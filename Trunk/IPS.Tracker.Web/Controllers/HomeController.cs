﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPS.Tracker.Web.Models;
using IPS.Tracker.Web.TrackerService;
using PagedList;

namespace IPS.Tracker.Web.Controllers
{
    public class HomeController : Controller
    {
        const int defectsPerPage = 10;
        //
        // GET: /Home/
        public ActionResult Index()
        {
            /*using (TrackerServiceClient client = new TrackerServiceClient())
            {
                WorkerDTO currentWorker = GetCurrentWorker(client);
                List<DefectCommentDTO> list = client.GetLastComments(currentWorker.Id, 50);
                ViewBag.UserId = currentWorker.Id;
                return View(list);
            }*/
            return View();
        }

        public ActionResult ReportNewDefect()
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                DefectDTO defect = client.InitializeNewDefect();
                WorkerDTO currentWorker = GetCurrentWorker(client);
                defect.AssigneeId = currentWorker.Id;
                DefectViewModel viewModel = new DefectViewModel(defect);
                viewModel.Workers = client.GetActiveWorkers();
                return View(viewModel);
            }
        }

        [HttpPost()]
        public ActionResult ReportNewDefect(DefectViewModel viewModel)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                WorkerDTO currentWorker = GetCurrentWorker(client);
                byte[] buffer = null;
                string contentType = String.Empty;

                if (viewModel.UploadedFile != null)
                {
                    Stream str = viewModel.UploadedFile.InputStream;
                    buffer = new byte[viewModel.UploadedFile.ContentLength];
                    contentType = viewModel.UploadedFile.FileName.Substring(viewModel.UploadedFile.FileName.LastIndexOf('.') + 1);
                    str.Read(buffer, 0, viewModel.UploadedFile.ContentLength);
                }

                int assigneeId;

                if (viewModel.SelectedWorkOrderId.HasValue)
                    assigneeId = client.GetDefaultAssigneeId(viewModel.SelectedWorkOrderId.Value);
                else
                    assigneeId = currentWorker.Id;

                DefectDTO defect = client.ReportNewDefect(viewModel.Summary, viewModel.Description, viewModel.SelectedWorkOrderId, assigneeId, currentWorker.Id, 1, null, buffer, contentType);
                return RedirectToAction("DefectReported", "Home", new { defectId = defect.Id });
            }
        }

        private WorkerDTO GetCurrentWorker(TrackerServiceClient client)
        {
            string currentUsername = User.Identity.Name.Split('\\')[1];
            List<WorkerDTO> workers = client.GetActiveWorkers();
            WorkerDTO currentWorker = workers.Where(w => w.Username.ToUpper() == currentUsername.ToUpper()).First();
            return currentWorker;
        }

        public ActionResult WorkOrders()
        {
            //var result = new List<object>();

            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<WorkOrderDTO> workOrders = client.GetActiveWorkOrders();

                //foreach (WorkOrderDTO wo in workOrders)
                //    result.Add(new { Id = wo.Id, Name = wo.Name });

                return Json(workOrders, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLeaderId(int workOrderId)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                int leaderId = client.GetDefaultAssigneeId(workOrderId);
                return Json(leaderId, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DefectReported(int defectId)
        {
            ViewBag.DefectId = defectId;
            return View();
        }

        public ActionResult ListProblemsByUser(int? userId, string stateDescription = "Aktivni", int page = 1)
        {
            ViewBag.EditMode = true;
            ViewBag.StateDescription = stateDescription;
            ViewBag.UserId = userId;
            // ViewBag.Page = page;

            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<DefectDTO> result;


                if (userId.HasValue)
                    //    result = client.GetDefectsByWorker(userId.Value, page - 1, defectsPerPage, stateDescription);
                    result = client.GetAllDefectsByWorker(userId.Value, stateDescription);
                else
                {
                    WorkerDTO currentWorker = GetCurrentWorker(client);
                    ViewBag.UserId = currentWorker.Id;
                    result = client.GetAllDefectsByWorker(currentWorker.Id, stateDescription);
                }

                // result = ExtractResultByState(stateDescription, result);

                return View(result);

                // return View(result.ToPagedList(page, defectsPerPage));
            }
        }

        /*
        private List<DefectDTO> ExtractResultByState(string stateDescription, List<DefectDTO> result)
        {
            switch (stateDescription)
            {
                case "Riješen":
                    result = result.Where(d => d.StateDescription == stateDescription).ToList();
                    break;
                case "Aktivni":
                    result = result.Where(d => d.StateDescription != "Riješen").ToList();
                    break;
                case "Moji aktivni":
                    result = result.Where(d => d.StateDescription != "Riješen" && d.AssigneeId == ViewBag.UserId).ToList();
                    break;
                default:
                    if (ViewBag.UserId == null)
                        result = result.Where(d => d.StateDescription != "Riješen").ToList();
                    else
                        result = result.Where(d => d.StateDescription != "Riješen" && d.AssigneeId == ViewBag.UserId).ToList();
                    break;
            }
            return result;
        }
        */

        public ActionResult ListProblemsByWorkOrder(int workOrderId, string stateDescription = "Aktivni", int page = 0)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<DefectDTO> defects = client.GetDefectsByWorkOrder(workOrderId, stateDescription, page);
                List<WorkOrderDTO> workOrders = client.GetAllWorkOrders();

                // defects = ExtractResultByState(stateDescription, defects);

                WorkOrderDTO selectedWorkOrder = workOrders.Where(w => w.Id == workOrderId).First();
                ViewBag.RadniNalog = selectedWorkOrder.Name;
                return View(defects);
            }
        }

        public ActionResult ListProblemsInSprint(bool onlyMine = false)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<DefectDTO> defects = client.GetMaxValueSprintDefects();
                WorkerDTO worker = GetCurrentWorker(client);
                ViewBag.IsAdministrator = worker.TrackerAdmin == "D";

                ListProblemsBoardViewModel viewModel = new ListProblemsBoardViewModel();

                viewModel.SprintCompletion = Math.Round((decimal)defects.Count(d => d.DefectState == "CLS") * 100 / defects.Count, 0);

                if (onlyMine)
                    viewModel.Defects = defects.Where(d => d.AssigneeId == worker.Id);
                else
                    viewModel.Defects = defects;

                if (defects.Count > 0)
                    viewModel.SprintNo = defects[0].SprintNo;

                return View(viewModel);
            }
        }

        public ActionResult Details(int id)
        {
            ViewBag.Id = id;
            string tempFilePath = "~/TempFiles/";

            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                DefectDTO defect = client.GetDefectById(id);
                defect.LinkedDefects = new List<DefectDTO>();

                foreach (int no in defect.LinkedDefectNumbers)
                {
                    DefectDTO d = client.GetDefectById(no);

                    if (d != null)
                        defect.LinkedDefects.Add(d);
                }

                WorkerDTO currentWorker = GetCurrentWorker(client);

                //if (defect.DefectFollowers.Any(df => df.FollowerId == currentWorker.Id))
                //    ViewBag.ShowEditBUtton = true;
                //else
                //    ViewBag.ShowEditBUtton = false;

                if (defect.WorkOrderId.HasValue)
                {
                    int leaderId = client.GetDefaultAssigneeId(defect.WorkOrderId.Value);
                    ViewBag.ShowEditButton = (defect.AssigneeId == currentWorker.Id) || (leaderId == currentWorker.Id) || (currentWorker.TrackerAdmin == "D");
                }
                else
                {
                    ViewBag.ShowEditButton = (defect.AssigneeId == currentWorker.Id) || (defect.ReporterId == currentWorker.Id) || (currentWorker.TrackerAdmin == "D");
                }

                if (defect.DefectFile != null)
                {
                    string path = Path.Combine(Server.MapPath(tempFilePath), "temp." + defect.DefectFileType);
                    ViewBag.DefectFilePath = Url.Content(String.Format("{0}{1}", tempFilePath, "temp." + defect.DefectFileType));
                    FileStream fs = new FileStream(path, FileMode.Create);
                    fs.Write(defect.DefectFile, 0, defect.DefectFile.Length);
                    fs.Close();
                }

                if (defect == null)
                    return View("PageNotExist");
                else
                    return View(defect);
            }
        }

        public ActionResult Edit(int id)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                DefectDTO defect = client.GetDefectById(id);
                WorkerDTO currentWorker = GetCurrentWorker(client);
                bool planningAllowed = false;

                if (defect.WorkOrderId.HasValue)
                {
                    int leaderId = client.GetDefaultAssigneeId(defect.WorkOrderId.Value);
                    planningAllowed = (leaderId == currentWorker.Id);
                }
                else
                    planningAllowed = (defect.ReporterId == currentWorker.Id);

                DefectViewModel viewModel = new DefectViewModel(defect);
                viewModel.Workers = client.GetActiveWorkers();
                viewModel.PlanningAllowed = planningAllowed || currentWorker.TrackerAdmin == "D";
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Edit(DefectViewModel viewModel)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                WorkerDTO currentWorker = GetCurrentWorker(client);
                DefectDTO defect = client.SaveDefect(viewModel.Id, viewModel.Summary, viewModel.Description, viewModel.SelectedWorkOrderId, viewModel.AssigneeId, currentWorker.Id, viewModel.SelectedPriorityId, viewModel.SprintNumber, viewModel.StateDescription);

                if (!String.IsNullOrEmpty(viewModel.EditCommentText))
                {
                    client.CommentOnDefect(viewModel.Id, currentWorker.Id, viewModel.EditCommentText);
                }

                return RedirectToAction("Details", "Home", new { id = viewModel.Id });
                /*    viewModel = new DefectViewModel(defect);
                    viewModel.Workers = client.GetActiveWorkers();
                    ModelState.AddModelError(String.Empty,
                    return View(viewModel);*/
            }
        }

        [HttpPost]
        public ActionResult NewComment(int id, string text)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                WorkerDTO currentWorker = GetCurrentWorker(client);
                DefectCommentDTO comment = client.CommentOnDefect(id, currentWorker.Id, text);
                return PartialView("CommentRow", comment);
            }
        }

        public ActionResult Search(string criteria)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<DefectDTO> results = client.SearchDefects(criteria);
                ViewBag.EditMode = false;
                ViewBag.DefectsPerPage = 20;
                ViewBag.SearchTerm = criteria;
                return View(results);
            }
        }

        public ActionResult CloseSprint()
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                // List<DefectDTO> defects = client.GetMaxValueSprintDefects();
                WorkerDTO worker = GetCurrentWorker(client);

                if (worker.TrackerAdmin == "D")
                    client.CloseSprint();

                return RedirectToAction("ListProblemsInSprint");
            }
        }
        
        public ActionResult Releases()
        {
            return View();
        }

        public ActionResult NewRelease()
        {
            return View();
        }

        [HttpPost]
        public ActionResult NewRelease(ReleaseViewModel release)
        {
            return View();
        }
    }
}