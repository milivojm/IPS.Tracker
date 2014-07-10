using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IPS.Tracker.Data
{
    public partial class Defect
    {
        public Defect()
        {
            _DefectDate = DateTime.Now;
            _DefectState = "OPN";
            _Priority = 0;
        }

        public string PriorityDescription
        {
            get
            {
                switch (Priority)
                {
                    case 0:
                        return "Nizak";
                    case 1:
                        return "Srednji";
                    case 2:
                        return "Visok";
                    default:
                        return "Nepoznat";
                }
            }
        }

        public string StateDescription
        {
            get
            {
                switch (_DefectState)
                {
                    case "OPN":
                        return "Novi";
                    case "PRO":
                        return "Ispravljen";
                    case "TST":
                        return "Testirati";
                    case "INV":
                        return "Neispravan";
                    case "CLS":
                        return "Riješen";
                    default:
                        return "Nepoznat";
                }
            }
            set
            {
                switch (value)
                {
                    case "Novi":
                        DefectState = "OPN";
                        break;
                    case "Ispravljen":
                        DefectState = "PRO";
                        break;
                    case "Testirati":
                        DefectState = "TST";
                        break;
                    case "Neispravan":
                        DefectState = "INV";
                        break;
                    case "Riješen":
                        DefectState = "CLS";
                        break;
                }
            }
        }

        public DefectComment Change(string summary, string description, int? workOrderId, int assigneeId, int changedById, short priority, string state)
        {
            DefectComment comment = new DefectComment();
            comment.CommentatorId = changedById;
            comment.CommentDate = DateTime.Now;
            comment.DefectId = this.Id;

            if (this.DefectComments == null)
                this.DefectComments = new System.Data.Objects.DataClasses.EntityCollection<DefectComment>();

            if (this.Summary != summary)
            {
                comment.AddChange("naslov", Summary, summary);
                this.Summary = summary;
            }

            if (this.Description != description)
            {
                comment.AddChange("opis", Description, description);
                this.Description = description;
                CheckForNewFollowers(summary);
            }

            if (this.WorkOrderId != workOrderId)
            {
                string oldWorkOrderLabel;

                if (this.WorkOrder != null)
                    oldWorkOrderLabel = this.WorkOrder.Label;
                else
                    oldWorkOrderLabel = "(prazno)";

                this.WorkOrderId = workOrderId;
                comment.AddChange("radni nalog", oldWorkOrderLabel, WorkOrder.Label);
            }

            if (this.AssigneeId != assigneeId)
            {
                string oldAssignee = this.Assignee.Name;
                this.AssigneeId = assigneeId;
                SubscribeFollower(assigneeId);
                comment.AddChange("zadužen", oldAssignee, Assignee.Name);
            }

            if (this.Priority != priority)
            {
                string oldPriority = this.PriorityDescription;
                this.Priority = priority;
                comment.AddChange("prioritet", oldPriority, this.PriorityDescription);
            }

            if (this.StateDescription != state)
            {
                comment.AddChange("stanje", StateDescription, state);
                this.StateDescription = state;
            }

            DefectComments.Add(comment);
            return comment;
        }

        public void SubscribeFollower(int followerId)
        {
            if (DefectFollowers == null)
                DefectFollowers = new System.Data.Objects.DataClasses.EntityCollection<DefectFollower>();

            if (DefectFollowers.Count(df => df.FollowerId == followerId) == 0)
            {
                DefectFollower df = new DefectFollower();
                df.FollowerId = followerId;
                df.Defect = this;
                DefectFollowers.Add(df);
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

        public DefectComment AddComment(int workerId, string commentText)
        {
            if (DefectComments == null)
                DefectComments = new System.Data.Objects.DataClasses.EntityCollection<DefectComment>();

            DefectComment dc = new DefectComment();
            dc.CommentDate = DateTime.Now;
            dc.CommentatorId = workerId;
            dc.Text = commentText;
            dc.DefectId = this.Id;

            DefectComments.Add(dc);
            CheckForNewFollowers(dc.Text);

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
                            SubscribeFollower(taggedWorker.Id);
                    }
                }
            }
        }
    }

    public partial class WorkOrder
    {
        public string Label
        {
            get
            {
                return String.Format("{0}/{1}", Year, Subnumber.HasValue ? String.Format("{0}/{1}", OrdinalNumber, Subnumber) : OrdinalNumber.ToString());
            }
        }
    }

    public partial class DefectComment
    {
        public void AddChange(string propertyName, string oldValue, string newValue)
        {
            if (String.IsNullOrEmpty(Text))
                Text = String.Format("{0}: {1} => {2}", propertyName, oldValue, newValue);
            else
                Text += String.Format(", {0}: {1} => {2}", propertyName, oldValue, newValue);
        }
    }

    public partial class Worker
    {
        public static List<Worker> AllWorkers { get; set; }
    }
}
