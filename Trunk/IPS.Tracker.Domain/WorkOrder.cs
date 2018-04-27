using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IPS.Tracker.Domain
{
    public class WorkOrder
    {
        public int Id
        {
            get; private set;
        }

        public int OrdinalNumber
        {
            get; private set;
        }

        public short Year
        {
            get; private set;
        }

        public string Leader
        {
            get; private set;
        }

        public string Name
        {
            get; private set;
        }

        public string Description
        {
            get; private set;
        }

        public short? Subnumber
        {
            get; private set;
        }

        public string Label
        {
            get
            {
                return string.Format("{0}/{1}", Year, Subnumber.HasValue ? string.Format("{0}/{1}", OrdinalNumber, Subnumber) : OrdinalNumber.ToString());
            }
        }

        public string Completed { get; private set; }
    }
}
