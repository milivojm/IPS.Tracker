using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using IPS.Tracker.Data;
using AutoMapper;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Web;
using System.ServiceModel.Activation;
using System.Configuration;
using System.Text.RegularExpressions;

namespace IPS.Tracker.WCF
{
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
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

        public DefectDTO ReportNewDefect(string summary, string description, int? workOrderId, int assigneeId, int reporterId, short priority, int? sprint, byte[] defectFile, string fileContentType)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                Worker.AllWorkers = e.Workers.ToList();

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
                newDefect.SubscribeFollower(assigneeId);
                newDefect.SubscribeFollower(reporterId);
                newDefect.CheckForNewFollowers(description);
                newDefect.DefectFileType = fileContentType;
                e.Defects.AddObject(newDefect);
                e.SaveChanges();

                SendNotification(newDefect);
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

        public List<WorkerDTO> GetActiveWorkers()
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                var query = from w in e.Workers
                            where w.Retired == "N"
                            orderby w.Name
                            select w;

                return Mapper.Map<List<WorkerDTO>>(query.ToList());
            }
        }

        private void SendNotification(Defect defect)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("ips@gzr.hr", "ips@gzr.hr", Encoding.Unicode);

            string detailsUrl = ConfigurationManager.AppSettings["DetailsUrl"];

            foreach (DefectFollower df in defect.DefectFollowers)
                mailMessage.To.Add(new MailAddress(df.Follower.Email));

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
            using (TrackerEntities e = new TrackerEntities())
            {
                var query = from d in e.Defects
                            where d.AssigneeId == workerId
                            select d;

                if (state == "Riješen")
                    query = query.Where(d => d.DefectState == "CLS");

                if (state == "Aktivni")
                    query = query.Where(d => d.DefectState != "CLS");

                query = query.OrderByDescending(d => d.Priority).ThenBy(d => d.DefectDate).Skip(pageNumber * defectsPerPage).Take(defectsPerPage);

                return Mapper.Map<List<DefectDTO>>(query.ToList());
            }
        }


        public DefectDTO GetDefectById(int defectId)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                var query = from w in e.Defects
                            where w.Id == defectId
                            select w;

                return Mapper.Map<DefectDTO>(query.FirstOrDefault());
            }
        }

        //DefectDTO SaveDefect(int id, string summary, string description, int? workOrderId, int assigneeId, int changedById, short priority, int? sprint, string state);
        public DefectDTO SaveDefect(int id, string summary, string description, int? workOrderId, int assigneeId, int changedById, short priority, int? sprint, string state)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                Worker.AllWorkers = e.Workers.ToList();

                Defect defect = (from d in e.Defects
                                 where d.Id == id
                                 select d).First();

                DefectComment comment = defect.Change(summary, description, workOrderId, assigneeId, changedById, priority, sprint, state);

                e.SaveChanges();
                SendNotification(comment);
                return Mapper.Map<DefectDTO>(defect);
            }
        }

        private void SendNotification(DefectComment defectComment)
        {
            if (defectComment == null)
                return;

            Defect defect = defectComment.Defect;

            if (defect.DefectFollowers == null || defect.DefectFollowers.Count == 0)
                return;

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("ips@gzr.hr", "ips@gzr.hr", Encoding.Unicode);

            foreach (DefectFollower df in defect.DefectFollowers)
            {
                if (df.Follower != defectComment.Commentator)
                    mailMessage.To.Add(new MailAddress(df.Follower.Email));
            }

            // ne treba nikog obavijestiti
            if (mailMessage.To.Count == 0)
                return;

            string detailsUrl = ConfigurationManager.AppSettings["DetailsUrl"];

            mailMessage.Subject = String.Format("[{0}] {1} (#{2})", defect.WorkOrder != null ? defect.WorkOrder.Name : "Neodređeni radni nalog", defect.Summary, defect.Id);
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
            using (TrackerEntities e = new TrackerEntities())
            {
                var query = from d in e.Defects
                            where d.WorkOrderId == workOrderId
                            select d;

                if (state == "Riješen")
                    query = query.Where(d => d.DefectState == "CLS");

                if (state == "Aktivni")
                    query = query.Where(d => d.DefectState != "CLS");

                query = query.OrderByDescending(d => d.Priority).ThenBy(d => d.DefectDate);

                return Mapper.Map<List<DefectDTO>>(query.ToList());
            }
        }

        public DefectCommentDTO CommentOnDefect(int defectId, int workerId, string commentText)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                Worker.AllWorkers = e.Workers.ToList();
                Defect defect = e.Defects.First(d => d.Id == defectId);
                DefectComment dc = defect.AddComment(workerId, commentText);
                e.SaveChanges();
                SendNotification(dc);
                return Mapper.Map<DefectCommentDTO>(dc);
            }
        }

        private string GetPhysicalPath(string path)
        {
            return HttpContext.Current.Server.MapPath(path);
        }


        public List<DefectDTO> SearchDefects(string searchTerm)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                int taskId;
                bool succ = int.TryParse(searchTerm, out taskId);
                IQueryable<Defect> query;

                if (succ)
                    query = from d in e.Defects
                            where d.Id == taskId
                            select d;

                else //prvo potrazi broj sprinta
                {
                    Regex sprintReg = new Regex("[A-Za-z]*(?i)Sprint.*[0-9].*");

                    if (sprintReg.IsMatch(searchTerm))
                    {
                        string SprintNo = Regex.Replace(searchTerm, @"\D", "");

                        int sprint = int.Parse(SprintNo);

                        query = from d in e.Defects
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

                            query = from d in e.Defects
                                    where d.WorkOrder.OrdinalNumber == workOrderNo
                                    orderby d.DefectDate descending
                                    select d;
                        }
                        else
                        {
                            query = from d in e.Defects
                                    where d.Summary.ToUpper().Contains(searchTerm.ToUpper())
                                    || d.Description.ToUpper().Contains(searchTerm.ToUpper())
                                    orderby d.DefectDate descending
                                    select d;
                        }

                    }

                }

                return Mapper.Map<List<DefectDTO>>(query.ToList());
            }
        }


        public List<DefectCommentDTO> GetLastCommentsByWorker(int currentWorkerId, int commentNumber)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                IQueryable<DefectComment> query = (from dc in e.DefectComments
                                                   from df in e.DefectFollowers
                                                   where dc.DefectId == df.DefectId
                                                   && df.FollowerId == currentWorkerId
                                                   orderby dc.CommentDate descending
                                                   select dc).Take(commentNumber);

                var newDefects = (from d in e.Defects
                                  from df in e.DefectFollowers
                                  where df.FollowerId == currentWorkerId
                                  && d.Id == df.DefectId
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
        }

        public List<DefectCommentDTO> GetLastComments(int commentNumber)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                IQueryable<DefectComment> query = (from dc in e.DefectComments
                                                   from df in e.DefectFollowers
                                                   where dc.DefectId == df.DefectId
                                                   orderby dc.CommentDate descending
                                                   select dc).Take(commentNumber);

                var newDefects = (from d in e.Defects
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
        }

        public List<WorkOrderDTO> GetAllWorkOrders()
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                var query = from wo in e.WorkOrders
                            orderby wo.Year, wo.OrdinalNumber descending
                            select wo;

                return Mapper.Map<List<WorkOrderDTO>>(query.ToList());
            }
        }

        public List<DefectDTO> GetMaxValueSprintDefects()
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                var maxvalue = (from w in e.Defects
                                select w.SprintNo).Max();

                var query = from w in e.Defects
                            where w.SprintNo == maxvalue
                            select w;

                return Mapper.Map<List<DefectDTO>>(query.ToList());
            }
        }

        public void CloseSprint()
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                var maxvalue = (from w in e.Defects
                                select w.SprintNo).Max();

                var query = from w in e.Defects
                            where w.SprintNo == maxvalue
                            && w.DefectState != "CLS"
                            select w;

                foreach (var openDefect in query)
                    openDefect.SprintNo = (short)((maxvalue ?? 0) + 1);

                e.SaveChanges();
            }
        }

        public List<DefectDTO> GetAllDefectsByWorker(int workerId, string state)
        {
            using (TrackerEntities e = new TrackerEntities())
            {
                var query = from d in e.Defects
                            where d.AssigneeId == workerId
                            select d;

                if (state == "Riješen")
                    query = query.Where(d => d.DefectState == "CLS");

                if (state == "Aktivni")
                    query = query.Where(d => d.DefectState != "CLS");

                query = query.OrderByDescending(d => d.Priority).ThenBy(d => d.DefectDate);

                return Mapper.Map<List<DefectDTO>>(query.ToList());
            }
        }
    }
}
