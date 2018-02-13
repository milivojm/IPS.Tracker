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

        public DefectViewModel() { }

        public DefectViewModel(DefectDTO defect)
        {
            _priorities = new List<Priority>();
            _priorities.Add(new Priority() { Id = 0, Name = "Nizak" });
            _priorities.Add(new Priority() { Id = 1, Name = "Srednji" });
            _priorities.Add(new Priority() { Id = 2, Name = "Visok" });

            _defectStates = new List<string>();
            _defectStates.Add("Novi");
            _defectStates.Add("Ispravljen");
            // _defectStates.Add("Testirati");
            _defectStates.Add("Neispravan");
            _defectStates.Add("Riješen");

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

            Comments = defect.DefectComments.OrderByDescending(d => d.CommentDate).ToList();
            Followers = defect.DefectFollowers;

            if (Comments == null)
                Comments = new List<DefectCommentDTO>();
        }

        [Display(Name = "Prioritet *")]
        public short SelectedPriorityId { get; set; }

        public IEnumerable<SelectListItem> Priorities
        {
            get
            {
                return new SelectList(_priorities, "Id", "Name");
            }
        }

        [Required(ErrorMessage="Naziv je obavezan")]
        [StringLength(50,ErrorMessage="Najviše 50 znakova")]
        [Display(Name = "Naziv *")]
        public string Summary { get; set; }

        [Required(ErrorMessage="Opis zadatka je obavezan")]
        [Display(Name = "Opis zadatka *")]
        [AllowHtml()]
        public string Description { get; set; }

        [Display(Name = "Radni nalog")]
        public string SelectedWorkOrderName { get; set; }

        public int? SelectedWorkOrderId { get; set; }

        [Required(ErrorMessage="Zaduženje je obavezno")]
        [Display(Name = "Zadužen *")]
        public int AssigneeId { get; set; }

        [Display(Name = "Sprint")]
        public int? SprintNumber { get; set; }

        public HttpPostedFileBase UploadedFile { get; set; }

        public List<WorkerDTO> Workers
        {
            get { return _workers; }
            set { _workers = value; }
        }

        public IEnumerable<SelectListItem> Assignees
        {
            get
            {
                return new SelectList(_workers, "Id", "Name", AssigneeId);
            }
        }

        [Display(Name = "Stanje")]
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

        [Display(Name = "Prijavio")]
        public string ReporterName { get; set; }

        public List<DefectCommentDTO> Comments { get; set; }

        public List<WorkerDTO> Followers { get; set; }

        public string EditCommentText { get; set; }

        public bool PlanningAllowed { get; set; }
    }
}