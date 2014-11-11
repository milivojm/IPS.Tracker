using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IPS.Tracker.Web.TrackerService
{
    public partial class WorkOrderDTO
    {
        public string Label
        {
            get
            {
                if (SubNumber.HasValue)
                    return String.Format("{0}/{1}/{2} - {3}", Year, OrdinalNumber, SubNumber, Name);
                else
                    return String.Format("{0}/{1} - {2}", Year, OrdinalNumber, Name);
            }
        }
    }
}