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
        DefectDTO ReportNewDefect(string summary, string description, int? workOrderId, int assigneeId, int reporterId, short priority, int? sprint, Byte[] defectFile, string defectFileType);

        [OperationContract]
        DefectDTO SaveDefect(int id, string summary, string description, int? workOrderId, int assigneeId, int changedById, short priority, int? sprint, string state);

        [OperationContract]
        int GetDefaultAssigneeId(int workOrderId);

        [OperationContract]
        List<WorkOrderDTO> GetActiveWorkOrders();

        [OperationContract]
        List<WorkOrderDTO> GetAllWorkOrders();

        [OperationContract]
        List<WorkerDTO> GetActiveWorkers();

        [OperationContract]
        List<DefectDTO> GetDefectsByWorker(int workerId, int pageNumber, int defectsPerPage, string state);

        [OperationContract]
        List<DefectDTO> GetAllDefectsByWorker(int workerId, string state);

        [OperationContract]
        DefectDTO GetDefectById(int defectId);

        [OperationContract]
        List<DefectDTO> GetDefectsByWorkOrder(int workOrderId, string state, int page);

        [OperationContract]
        DefectCommentDTO CommentOnDefect(int defectId, int workerId, string commentText);

        [OperationContract]
        List<DefectDTO> SearchDefects(string searchTerm);

        [OperationContract]
        List<DefectCommentDTO> GetLastCommentsByWorker(int currentWorkerId, int commentNumber);

        [OperationContract]
        List<DefectCommentDTO> GetLastComments(int commentNumber);
    }
}
