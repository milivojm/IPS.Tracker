using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPS.Tracker.Web.TrackerService;

namespace IPS.Tracker.Web.Models
{
    public class DefectViewModel
    {
        private List<Priority> _priorities;
        private List<WorkerDTO> _workers;
        private List<string> _defectStates;
        private List<ReleaseDTO> _releases;

        public DefectViewModel() { }

        public DefectViewModel(DefectDTO defect)
        {
            _priorities = new List<Priority>();
            _priorities.Add(new Priority() { Id = 0, Name = "Low" });
            _priorities.Add(new Priority() { Id = 1, Name = "Normal" });
            _priorities.Add(new Priority() { Id = 2, Name = "High" });

            _defectStates = new List<string>();
            _defectStates.Add("Open");
            _defectStates.Add("Needs review");
            _defectStates.Add("Reviewed");
            _defectStates.Add("Needs QA test");
            _defectStates.Add("Needs improvements");
            _defectStates.Add("Resolved");

            SelectedPriorityId = defect.Priority;
            Summary = defect.Summary;
            Description = defect.Description;
            SelectedWorkOrderName = defect.WorkOrderName;
            SelectedWorkOrderId = defect.WorkOrderId;
            AssigneeId = defect.AssigneeId;
            SelectedPriorityId = defect.Priority;
            StateDescription = defect.StateDescription;
            Id = defect.Id;
            ReporterName = defect.ReporterName;

            SprintNumber = defect.SprintNo;
            ReleaseVersion = defect.ReleaseVersion;
            ReleaseId = defect.ReleaseId;

            Comments = defect.DefectComments.OrderByDescending(d => d.CommentDate).ToList();
            Followers = defect.DefectFollowers;

            if (Comments == null)
                Comments = new List<DefectCommentDTO>();
        }

        [Display(Name = "Priority *")]
        public short SelectedPriorityId { get; set; }

        public IEnumerable<SelectListItem> Priorities
        {
            get
            {
                return new SelectList(_priorities, "Id", "Name");
            }
        }

        [Required(ErrorMessage="Summary is mandatory")]
        [StringLength(50,ErrorMessage="Max 50 characters")]
        [Display(Name = "Summary *")]
        public string Summary { get; set; }

        [Required(ErrorMessage="Description is mandatory")]
        [Display(Name = "Description *")]
        [AllowHtml()]
        public string Description { get; set; }

        [Display(Name = "Work order")]
        public string SelectedWorkOrderName { get; set; }

        public int? SelectedWorkOrderId { get; set; }

        [Required(ErrorMessage="Assignee is mandatory")]
        [Display(Name = "Assigned to *")]
        public int AssigneeId { get; set; }

        [Display(Name = "Sprint")]
        public int? SprintNumber { get; set; }

        [Display(Name = "Release version")]
        public string ReleaseVersion { get; set; }

        public int? ReleaseId { get; set; }

        public HttpPostedFileBase UploadedFile { get; set; }

        public List<WorkerDTO> Workers
        {
            get { return _workers; }
            set { _workers = value; }
        }

        public List<ReleaseDTO> Releases
        {
            get { return _releases; }
            set { _releases = value; }
        }

        public IEnumerable<SelectListItem> ReleaseVersions
        {
            get
            {
                return new SelectList(_releases, "ReleaseVersion", "ReleaseVersion", selectedValue: ReleaseVersion);                                
            }
        }

        public IEnumerable<SelectListItem> Assignees
        {
            get
            {
                return new SelectList(_workers, "Id", "Name", AssigneeId);
            }
        }

        [Display(Name = "State")]
        public string StateDescription
        {
            get;
            set;
        }

        public IEnumerable<SelectListItem> StateDescriptions
        {
            get
            {
                return new SelectList(_defectStates, StateDescription);
            }
        }

        public int Id { get; set; }

        [Display(Name = "Reported by")]
        public string ReporterName { get; set; }

        public List<DefectCommentDTO> Comments { get; set; }

        public List<WorkerDTO> Followers { get; set; }

        public string EditCommentText { get; set; }

        public bool PlanningAllowed { get; set; }
    }
}