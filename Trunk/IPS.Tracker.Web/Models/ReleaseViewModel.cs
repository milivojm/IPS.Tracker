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
        public string ReleaseNo { get; set; }
        public DateTime? EstDateOfRelease { get; set; }                       
        public List<string> ReleaseListDefectId { get; set; }
    }
}