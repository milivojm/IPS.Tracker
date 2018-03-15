using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPS.Tracker.Domain
{
    public interface ITrackerRepository
    {
        IQueryable<Defect> Defects { get; }
        IQueryable<WorkOrder> WorkOrders { get; }
        IQueryable<Worker> Workers { get; }
        void Save();
        void AddDefect(Defect defect);
        void AddRelease(Release release);
    }
}
