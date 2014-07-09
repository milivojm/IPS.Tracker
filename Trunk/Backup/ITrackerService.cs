using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace IPS.Tracker.WCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface ITrackerService
    {
        [OperationContract]
        DefectDTO InitializeNewDefect();

        [OperationContract]
        DefectDTO ReportNewDefect(string summary, string description, int? workOrderId, int assigneeId, int reporterId, short priority, Byte[] defectFile);

        [OperationContract]
        int GetDefaultAssigneeId(int workOrderId);

        [OperationContract]
        List<WorkOrderDTO> GetActiveWorkOrders();
    }
}
