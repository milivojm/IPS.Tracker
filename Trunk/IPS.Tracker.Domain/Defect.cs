using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IPS.Tracker.Domain
{
    public class Defect
    {
        private DateTime _defectDate;
        private string _defectState;
        private short _priority;
        private List<DefectComment> _comments { get; set; }
        private List<Worker> _followers { get; set; }

        public Defect()
        {
            _defectDate = DateTime.Now;
            _defectState = DefectStateConstants.Open;
            _priority = 0;
            _comments = new List<DefectComment>();
            _followers = new List<Worker>();
        }

        public string PriorityDescription
        {
            get
            {
                switch (_priority)
                {
                    case 0:
                        return "Low";
                    case 1:
                        return "Normal";
                    case 2:
                        return "High";
                    default:
                        return "Unknown";
                }
            }
        }

        public string StateDescription
        {
            get
            {
                switch (_defectState)
                {
                    case DefectStateConstants.Open:
                        return "Open";
                    case DefectStateConstants.NeedsReview:
                        return "Needs review";
                    case DefectStateConstants.Reviewed:
                        return "Reviewed";
                    case DefectStateConstants.NeedsQATesting:
                        return "Needs QA test";
                    case DefectStateConstants.NeedsImprovements:
                        return "Needs improvements";
                    case DefectStateConstants.Resolved:
                        return "Resolved";
                    default:
                        return "Unknown";
                }
            }
            set
            {
                switch (value)
                {
                    case "Open":
                        _defectState = DefectStateConstants.Open;
                        break;
                    case "Needs review":
                        _defectState = DefectStateConstants.NeedsReview;
                        break;
                    case "Reviewed":
                        _defectState = DefectStateConstants.Reviewed;
                        break;
                    case "Needs QA test":
                        _defectState = DefectStateConstants.NeedsQATesting;
                        break;
                    case "Needs improvements":
                        _defectState = DefectStateConstants.NeedsImprovements;
                        break;
                    case "Resolved":
                        _defectState = DefectStateConstants.Resolved;
                        break;
                }
            }
        }

        public IEnumerable<DefectComment> DefectComments
        {
            get
            {
                return new ReadOnlyCollection<DefectComment>(_comments);
            }
        }

        public IEnumerable<Worker> DefectFollowers
        {
            get
            {
                return new ReadOnlyCollection<Worker>(_followers);
            }
        }

        public int Id { get; private set; }

        public virtual WorkOrder WorkOrder { get; private set; }

        public virtual Worker Assignee { get; private set; }

        public DefectComment Change(string summary, string description, WorkOrder newWorkOrder, Worker newAssignee, int changedById, short priority, int? sprint, string state)
        {
            DefectComment comment = new DefectComment();
            comment.CommentatorId = changedById;
            comment.CommentDate = DateTime.Now;
            comment.DefectId = Id;

            if (_comments == null)
                _comments = new List<DefectComment>();

            if (Summary != summary)
            {
                comment.AddChange("summary", Summary, summary);
                Summary = summary;
            }

            if (Description != description)
            {
                comment.AddChange("description", Description, description);
                Description = description;
                CheckForNewFollowers(summary);
            }

            if (newWorkOrder != null && WorkOrderId != newWorkOrder.Id)
            {
                string oldWorkOrderLabel;

                if (WorkOrder != null)
                    oldWorkOrderLabel = WorkOrder.Label;
                else
                    oldWorkOrderLabel = "(blank)";

                WorkOrderId = newWorkOrder.Id;
                comment.AddChange("work order", oldWorkOrderLabel, newWorkOrder.Label);
            }
            else if (newWorkOrder == null && WorkOrderId.HasValue)
            {
                // situacija kad se RN stavi na blank!
                comment.AddChange("work order", WorkOrder.Label, "(blank)");
                WorkOrderId = null;
            }

            if (SprintNo != sprint)
            {
                comment.AddChange("Sprint No", SprintNo.ToString(), sprint.ToString());
                SprintNo = (short?)sprint;
            }

            if (AssigneeId != newAssignee.Id)
            {
                string oldAssignee = Assignee.Name;
                AssigneeId = newAssignee.Id;
                SubscribeFollower(newAssignee);
                comment.AddChange("assignee", oldAssignee, newAssignee.Name);
            }

            if (_priority != priority)
            {
                string oldPriority = this.PriorityDescription;
                _priority = priority;
                comment.AddChange("priority", oldPriority, PriorityDescription);
            }

            if (StateDescription != state)
            {
                comment.AddChange("state", StateDescription, state);
                StateDescription = state;
            }

            if (comment.IsValid())
                _comments.Add(comment);
            else
                comment = null;

            return comment;
        }

        public void SubscribeFollower(Worker newFollower)
        {
            if (_followers == null)
                _followers = new List<Worker>();

            if (_followers.Count(df => df.Id == newFollower.Id) == 0)
            {
                _followers.Add(newFollower);
            }
        }

        public DateTime? LastCommentDate
        {
            get
            {
                if (DefectComments == null || DefectComments.Count() == 0)
                    return null;
                else
                    return DefectComments.Max(dc => dc.CommentDate);
            }
        }

        public short Priority
        {
            get
            {
                return _priority;
            }

            set
            {
                _priority = value;
            }
        }

        public string DefectState
        {
            get { return _defectState; }
            set { _defectState = value; }
        }

        public string Summary { get; set; }

        public string Description { get; set; }

        public int? WorkOrderId { get; set; }
        public int? ReleaseId { get; set; }

        public int? SprintNo { get; set; }

        public int AssigneeId { get; set; }

        public DefectComment AddComment(Worker commentator, string commentText)
        {
            if (_comments == null)
                _comments = new List<DefectComment>();

            DefectComment dc = new DefectComment();
            dc.CommentDate = DateTime.Now;
            dc.CommentatorId = commentator.Id;
            dc.Text = commentText;
            dc.DefectId = Id;

            _comments.Add(dc);
            CheckForNewFollowers(dc.Text);
            SubscribeFollower(commentator);

            return dc;
        }

        public void CheckForNewFollowers(string text)
        {
            Regex r = new Regex(@"@(\w+)\b");
            List<Worker> workers = Worker.AllWorkers;

            foreach (Match m in r.Matches(text))
            {
                if (m.Success)
                {
                    if (m.Groups.Count == 2)
                    {
                        string tag = m.Groups[1].Value;
                        Worker taggedWorker = workers.FirstOrDefault(w => w.Username.ToLower() == tag.ToLower());

                        if (taggedWorker != null)
                            SubscribeFollower(taggedWorker);
                    }
                }
            }
        }

        public List<int> GetLinkedDefectNumbers()
        {
            List<int> res = new List<int>();

            if (this.DefectComments != null)
            {
                foreach (DefectComment dc in DefectComments)
                {
                    res.AddRange(dc.GetLinkedDefectNumbers());
                }
            }

            if (this.Description != null)
            {
                Regex r = new Regex(@"#(\d+)\b");
                MatchCollection matchCol = r.Matches(Description);

                foreach (Match m in matchCol)
                {
                    res.Add(int.Parse(m.Groups[1].Value));
                }
            }

            return res.Distinct().ToList();
        }

        public int ReporterId { get; set; }

        public virtual Worker Reporter { get; private set; }

        public byte[] DefectFile { get; set; }

        public DateTime DefectDate { get; set; }

        public string DefectFileType { get; set; }

        public class ORMappings
        {
            public const string CommentsCollectionName = "_comments";
            public const string FollowersCollectionName = "_followers";

            public static Expression<Func<Defect, ICollection<DefectComment>>> Comments
            {
                get
                {
                    return d => d._comments;
                }
            }

            public static Expression<Func<Defect, ICollection<Worker>>> Followers
            {
                get
                {
                    return d => d._followers;
                }
            }
        }

        public static class DefectStateConstants
        {
            public const string Open = "OPN";
            public const string NeedsReview = "NRE";
            public const string Reviewed = "REV";
            public const string NeedsQATesting = "PRO";
            public const string Resolved = "CLS";
            public const string NeedsImprovements = "INV";
        }
    }
}
