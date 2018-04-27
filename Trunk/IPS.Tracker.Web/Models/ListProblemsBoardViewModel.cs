using IPS.Tracker.Web.TrackerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IPS.Tracker.Web.Models
{
    public class ListProblemsBoardViewModel
    {
        public IEnumerable<DefectDTO> Defects { get; set; }
        public string Message { get; set; }
        public decimal SprintCompletion { get; internal set; }
        public int? SprintNo { get; set; }
    }
}