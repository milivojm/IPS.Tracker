﻿using System;
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
        ReleaseDTO SaveRelease(string releaseVersion, DateTime? date);

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
        List<DefectDTO> GetDefectsBySearchTerm(string term);

        [OperationContract]
        List<DefectDTO> GetAllDefects();

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

        [OperationContract]
        List<DefectDTO> GetMaxValueSprintDefects();

        [OperationContract]
        void CloseSprint();

        [OperationContract]
        void AddDefectToRelease(DefectDTO dto);

        [OperationContract]
        List<DefectDTO> GetAllOpenDefects();

        [OperationContract]
        List<ReleaseDTO> GetAllReleases();

        [OperationContract]
        string GetReleaseVersion(int? releaseId);

        [OperationContract]
        void SaveDefectRelease(string releaseVersion, int defectId);

        [OperationContract]
        bool ReleaseVersionExists(string releaseVersion);
    }
}
