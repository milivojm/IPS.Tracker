using IPS.Tracker.Web.TrackerService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IPS.Tracker.Web.Models
{
    public class ReleaseViewModel
    {
        [Required(ErrorMessage ="Broj verzije je obavezan!")]
        [Display(Name = "Broj verzije")]
        public string ReleaseNo { get; set; }
        [Display(Name ="Procjena datuma dostupnosti")]
        public DateTime? EstDateOfRelease { get; set; }        
        [Display(Name ="Lista zadataka za verziju")]
        public string[] ReleaseListDefectId { get; set; }
    }
}