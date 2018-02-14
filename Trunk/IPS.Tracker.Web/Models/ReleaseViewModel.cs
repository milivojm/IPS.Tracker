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
        [Display(Name = "Broj verzije")]
        public int ReleaseNo { get; set; }
        [Display(Name ="Procjena datuma dostupnosti")]
        public DateTime? EstDateOfRelease { get; set; }
        [Display(Name ="Lista zadataka za verziju")]
        public List<DefectDTO> ReleaseListDefectDTO { get; set; }
    }
}