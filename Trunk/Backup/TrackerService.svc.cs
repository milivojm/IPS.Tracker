using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using IPS.Tracker.Data;
using AutoMapper;

namespace IPS.Tracker.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class TrackerService : ITrackerService
    {
        public DefectDTO InitializeNewDefect()
        {
            Defect defect = new Defect();
            return Mapper.Map<DefectDTO>(defect);
        }

        public int GetDefaultAssigneeId(int workOrderId)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                int res = (from wo in e.WorkOrders
                           from w in e.Workers
                           where wo.Leader.ToUpper() == w.Username.ToUpper()
                           && wo.Id == workOrderId
                           select w.Id).First();

                return res;
            }
        }

        public DefectDTO ReportNewDefect(string summary, string description, int? workOrderId, int assigneeId, int reporterId, short priority, byte[] defectFile)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                Defect newDefect = new Defect();
                newDefect.Summary = summary;
                newDefect.Description = description;
                newDefect.WorkOrderId = workOrderId;
                newDefect.AssigneeId = assigneeId;
                newDefect.ReporterId = reporterId;
                newDefect.Priority = priority;
                newDefect.DefectFile = defectFile;
                e.Defects.AddObject(newDefect);
                e.SaveChanges();
                return Mapper.Map<DefectDTO>(newDefect);
            }
        }

        public List<WorkOrderDTO> GetActiveWorkOrders()
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                var query = from wo in e.WorkOrders
                            where wo.Completed == "N"
                            orderby wo.Year, wo.OrdinalNumber descending
                            select wo;

                return Mapper.Map<List<WorkOrderDTO>>(query.ToList());
            }
        }
    }
}
