using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using AutoMapper;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Web;
using System.ServiceModel.Activation;
using System.Configuration;
using System.Text.RegularExpressions;
using IPS.Tracker.Domain;
using System.Data.Entity;

namespace IPS.Tracker.WCF
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class TrackerService : ITrackerService
    {
        private readonly ITrackerRepository _repository;

        //public TrackerService() { }

        public TrackerService(ITrackerRepository repository)
        {
            _repository = repository;
        }

        public DefectDTO InitializeNewDefect()
        {
            Defect defect = new Defect();
            return Mapper.Map<DefectDTO>(defect);
        }

        public int GetDefaultAssigneeId(int workOrderId)
        {
            int res = (from wo in _repository.WorkOrders
                       from w in _repository.Workers
                       where wo.Leader.ToUpper() == w.Username.ToUpper()
                       && wo.Id == workOrderId
                       select w.Id).First();

            return res;
        }

        public DefectDTO ReportNewDefect(string summary, string description, int? workOrderId, int assigneeId, int reporterId, short priority, int? sprint, byte[] defectFile, string fileContentType)
        {

            Worker.AllWorkers = _repository.Workers.ToList();

            Defect newDefect = new Defect();
            newDefect.Summary = summary;
            newDefect.Description = description;
            newDefect.WorkOrderId = workOrderId;
            newDefect.AssigneeId = assigneeId;
            newDefect.ReporterId = reporterId;
            newDefect.Priority = priority;
            //newDefect.SprintNo = (short) sprint;
            newDefect.SprintNo = (short?)sprint;
            newDefect.DefectFile = defectFile;
            Worker assignee = _repository.Workers.First(w => w.Id == assigneeId);
            Worker reporter = _repository.Workers.First(w => w.Id == reporterId);
            newDefect.SubscribeFollower(assignee);
            newDefect.SubscribeFollower(reporter);
            newDefect.CheckForNewFollowers(description);
            newDefect.DefectFileType = fileContentType;
            _repository.AddDefect(newDefect);
            _repository.Save();

            SendNotification(newDefect);
            return Mapper.Map<DefectDTO>(newDefect);
        }

        public List<WorkOrderDTO> GetActiveWorkOrders()
        {
            var query = from wo in _repository.WorkOrders
                        where wo.Completed == "N"
                        orderby wo.Year, wo.OrdinalNumber descending
                        select wo;

            return Mapper.Map<List<WorkOrderDTO>>(query.ToList());
        }

        public List<WorkerDTO> GetActiveWorkers()
        {
            var query = from w in _repository.Workers
                        where w.Retired == "N"
                        orderby w.Name
                        select w;

            return Mapper.Map<List<WorkerDTO>>(query.ToList());
        }

        private void SendNotification(Defect defect)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("ips@gzr.hr", "ips@gzr.hr", Encoding.Unicode);

            string detailsUrl = ConfigurationManager.AppSettings["DetailsUrl"];

            foreach (var df in defect.DefectFollowers)
                mailMessage.To.Add(new MailAddress(df.Email));

            mailMessage.Subject = String.Format("[{0}] {1} (#{2})", defect.WorkOrder != null ? defect.WorkOrder.Name : "Neodređeni radni nalog", defect.Summary, defect.Id);
            string html = File.ReadAllText(GetPhysicalPath("MailTemplates\\NewDefect.html"));
            mailMessage.IsBodyHtml = true;
            html = html.Replace("{Summary}", defect.Summary);
            html = html.Replace("{Description}", defect.Description.Replace("\n", "<br/>"));
            html = html.Replace("{Link}", detailsUrl + defect.Id.ToString());
            mailMessage.Body = html;
            SmtpClient client = new SmtpClient();
            // client.Credentials = CredentialCache.DefaultNetworkCredentials;
            client.SendAsync(mailMessage, defect.Id);
            // client.Send(mailMessage);
        }

        public List<DefectDTO> GetDefectsByWorker(int workerId, int pageNumber, int defectsPerPage, string state)
        {
            var query = from d in _repository.Defects
                        where d.AssigneeId == workerId
                        select d;

            if (state == "Riješen")
                query = query.Where(d => d.DefectState == "CLS");

            if (state == "Aktivni")
                query = query.Where(d => d.DefectState != "CLS");

            query = query.OrderByDescending(d => d.Priority).ThenBy(d => d.DefectDate).Skip(pageNumber * defectsPerPage).Take(defectsPerPage);

            return Mapper.Map<List<DefectDTO>>(query.ToList());
        }


        public DefectDTO GetDefectById(int defectId)
        {
            var query = from w in _repository.Defects.Include("_comments").Include("_followers")
                        where w.Id == defectId
                        select w;

            return Mapper.Map<DefectDTO>(query.FirstOrDefault());
        }

        //DefectDTO SaveDefect(int id, string summary, string description, int? workOrderId, int assigneeId, int changedById, short priority, int? sprint, string state);
        public DefectDTO SaveDefect(int id, string summary, string description, int? workOrderId, int assigneeId, int changedById, short priority, int? sprint, string state)
        {
            Worker.AllWorkers = _repository.Workers.ToList();

            Defect defect = (from d in _repository.Defects
                             where d.Id == id
                             select d).First();

            Worker assignee = _repository.Workers.First(w => w.Id == assigneeId);
            WorkOrder newWorkOrder = null;

            if (workOrderId.HasValue)
                newWorkOrder = _repository.WorkOrders.First(wo => wo.Id == workOrderId);

            DefectComment comment = defect.Change(summary, description, newWorkOrder, assignee, changedById, priority, sprint, state);

            _repository.Save();
            SendNotification(comment);
            return Mapper.Map<DefectDTO>(defect);
        }

        private void SendNotification(DefectComment defectComment)
        {
            if (defectComment == null)
                return;

            Defect defect = defectComment.Defect;

            if (defect.DefectFollowers == null || defect.DefectFollowers.Count() == 0)
                return;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("ips@gzr.hr", "ips@gzr.hr", Encoding.Unicode);

            foreach (var df in defect.DefectFollowers)
            {
                if (df.Id != defectComment.CommentatorId)
                    mailMessage.To.Add(new MailAddress(df.Email));
            }

            // ne treba nikog obavijestiti
            if (mailMessage.To.Count == 0)
                return;

            string detailsUrl = ConfigurationManager.AppSettings["DetailsUrl"];

            mailMessage.Subject = string.Format("[{0}] {1} (#{2})", defect.WorkOrder != null ? defect.WorkOrder.Name : "Neodređeni radni nalog", defect.Summary, defect.Id);
            string html = File.ReadAllText(GetPhysicalPath("MailTemplates\\NewComment.html"));
            mailMessage.IsBodyHtml = true;
            html = html.Replace("{Summary}", defect.Summary);
            html = html.Replace("{Description}", defectComment.Text.Replace("\n", "<br/>"));
            html = html.Replace("{Link}", detailsUrl + defect.Id.ToString());
            html = html.Replace("{Commentator}", defectComment.Commentator.Name);
            mailMessage.Body = html;
            SmtpClient client = new SmtpClient();
            // client.Credentials = CredentialCache.DefaultNetworkCredentials;
            client.SendAsync(mailMessage, defect.Id);
            //client.Send(mailMessage);
        }

        public List<DefectDTO> GetDefectsByWorkOrder(int workOrderId, string state, int page)
        {
            var query = from d in _repository.Defects
                        where d.WorkOrderId == workOrderId
                        select d;

            if (state == "Riješen")
                query = query.Where(d => d.DefectState == "CLS");

            if (state == "Aktivni")
                query = query.Where(d => d.DefectState != "CLS");

            query = query.OrderByDescending(d => d.Priority).ThenBy(d => d.DefectDate);

            return Mapper.Map<List<DefectDTO>>(query.ToList());
        }

        public DefectCommentDTO CommentOnDefect(int defectId, int workerId, string commentText)
        {
            Worker.AllWorkers = _repository.Workers.ToList();
            Defect defect = _repository.Defects.First(d => d.Id == defectId);
            Worker commentator = _repository.Workers.First(c => c.Id == workerId);
            DefectComment dc = defect.AddComment(commentator, commentText);
            _repository.Save();
            SendNotification(dc);
            return Mapper.Map<DefectCommentDTO>(dc);
        }

        private string GetPhysicalPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }


        public List<DefectDTO> SearchDefects(string searchTerm)
        {

            int taskId;
            bool succ = int.TryParse(searchTerm, out taskId);
            IQueryable<Defect> query;

            if (succ)
                query = from d in _repository.Defects
                        where d.Id == taskId
                        select d;

            else //prvo potrazi broj sprinta
            {
                Regex sprintReg = new Regex("[A-Za-z]*(?i)Sprint.*[0-9].*");

                if (sprintReg.IsMatch(searchTerm))
                {
                    string SprintNo = Regex.Replace(searchTerm, @"\D", "");

                    int sprint = int.Parse(SprintNo);

                    query = from d in _repository.Defects
                            where d.SprintNo == sprint
                            orderby d.DefectDate descending
                            select d;
                }

                else //ako ne nađeš sprint pod tim brojem radi ono što si prije radio
                {
                    Regex reg = new Regex(@"^#(\d+)$");

                    if (reg.IsMatch(searchTerm))
                    {
                        int workOrderNo = int.Parse(reg.Match(searchTerm).Groups[1].Value);

                        query = from d in _repository.Defects
                                where d.WorkOrder.OrdinalNumber == workOrderNo
                                orderby d.DefectDate descending
                                select d;
                    }
                    else
                    {
                        query = from d in _repository.Defects
                                where d.Summary.ToUpper().Contains(searchTerm.ToUpper())
                                || d.Description.ToUpper().Contains(searchTerm.ToUpper())
                                orderby d.DefectDate descending
                                select d;
                    }

                }

            }

            return Mapper.Map<List<DefectDTO>>(query.ToList());
        }


        public List<DefectCommentDTO> GetLastCommentsByWorker(int currentWorkerId, int commentNumber)
        {
            IQueryable<DefectComment> query = (from d in _repository.Defects
                                               from dc in d.DefectComments
                                               from df in d.DefectFollowers
                                               where df.Id == currentWorkerId
                                               orderby dc.CommentDate descending
                                               select dc).Take(commentNumber);

            var newDefects = (from d in _repository.Defects
                              from df in d.DefectFollowers
                              where df.Id == currentWorkerId
                              orderby d.DefectDate descending
                              select d).Take(commentNumber);

            List<DefectCommentDTO> comments = Mapper.Map<List<DefectCommentDTO>>(query.ToList());

            List<DefectCommentDTO> defectsOpened = new List<DefectCommentDTO>();

            foreach (Defect d in newDefects)
            {
                defectsOpened.Add(new DefectCommentDTO() { CommentatorName = d.Reporter.Name, CommentDate = d.DefectDate, DefectId = d.Id, DefectSummary = d.Summary, Text = "Otvorio", DefectDescription = d.Description });
            }

            List<DefectCommentDTO> result = comments.Union(defectsOpened).OrderByDescending(d => d.CommentDate).Take(commentNumber).ToList();
            return result;
        }

        public List<DefectCommentDTO> GetLastComments(int commentNumber)
        {

            IQueryable<DefectComment> query = (from d in _repository.Defects.Include("_comments").Include("_followers").ToList().AsQueryable()
                                               from dc in d.DefectComments
                                               from df in d.DefectFollowers
                                               orderby dc.CommentDate descending
                                               select dc).Take(commentNumber);

            var newDefects = (from d in _repository.Defects
                              orderby d.DefectDate descending
                              select d).Take(commentNumber);

            List<DefectCommentDTO> comments = Mapper.Map<List<DefectCommentDTO>>(query.ToList());

            List<DefectCommentDTO> defectsOpened = new List<DefectCommentDTO>();

            foreach (Defect d in newDefects)
            {
                defectsOpened.Add(new DefectCommentDTO() { CommentatorName = d.Reporter.Name, CommentDate = d.DefectDate, DefectId = d.Id, DefectSummary = d.Summary, Text = "Otvorio", DefectDescription = d.Description });
            }

            List<DefectCommentDTO> result = comments.Union(defectsOpened).OrderByDescending(d => d.CommentDate).Take(commentNumber).ToList();
            return result;
        }

        public List<WorkOrderDTO> GetAllWorkOrders()
        {
            var query = from wo in _repository.WorkOrders
                        orderby wo.Year, wo.OrdinalNumber descending
                        select wo;

            return Mapper.Map<List<WorkOrderDTO>>(query.ToList());
        }

        public List<DefectDTO> GetMaxValueSprintDefects()
        {
            var maxvalue = (from w in _repository.Defects
                            select w.SprintNo).Max();

            var query = from w in _repository.Defects
                        where w.SprintNo == maxvalue
                        select w;

            return Mapper.Map<List<DefectDTO>>(query.ToList());
        }

        public void CloseSprint()
        {
            var maxvalue = (from w in _repository.Defects
                            select w.SprintNo).Max();

            var query = from w in _repository.Defects
                        where w.SprintNo == maxvalue
                        && w.DefectState != "CLS"
                        select w;

            foreach (var openDefect in query)
                openDefect.SprintNo = (short)((maxvalue ?? 0) + 1);

            _repository.Save();
        }

        public List<DefectDTO> GetAllDefectsByWorker(int workerId, string state)
        {
            var query = from d in _repository.Defects
                        where d.AssigneeId == workerId
                        select d;

            if (state == "Riješen")
                query = query.Where(d => d.DefectState == "CLS");

            if (state == "Aktivni")
                query = query.Where(d => d.DefectState != "CLS");

            query = query.OrderByDescending(d => d.Priority).ThenBy(d => d.DefectDate);

            return Mapper.Map<List<DefectDTO>>(query.ToList());
        }

        public ReleaseDTO SaveRelease(int id, DateTime? date)
        {
            Release release = new Release();
            return Mapper.Map<ReleaseDTO>(release);
        }
    }
}
