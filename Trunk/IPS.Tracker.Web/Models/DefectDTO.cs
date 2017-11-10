using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace IPS.Tracker.Web.TrackerService
{
    public partial class DefectDTO
    {
        public string DefectDateString
        {
            get
            {
                return DefectDate.ToShortDateString();
            }
        }

        public List<DefectDTO> LinkedDefects { get; set; }

        public string AssigneeInitials
        {
            get
            {
                string[] names = AssigneeName.Split(' ');
                string initials = string.Empty;

                foreach (var name in names)
                {
                    initials += name[0];
                }

                return initials;
            }
        }

        public string PriorityClass
        {
            get
            {
                switch (Priority)
                {
                    case 2:
                        return "bg-danger";
                    default:
                        return string.Empty;
                }
            }
        }

        public DateTime? LastChangeDate
        {
            get
            {
                return DefectComments.Select(dc => dc.CommentDate).DefaultIfEmpty().Max();
            }
        }
    }
}