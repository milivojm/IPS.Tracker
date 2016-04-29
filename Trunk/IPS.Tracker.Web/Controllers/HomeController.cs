using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IPS.Tracker.Web.Models;
using IPS.Tracker.Web.TrackerService;

namespace IPS.Tracker.Web.Controllers
{
    public class HomeController : Controller
    {
        const int defectsPerPage = 10;
        //
        // GET: /Home/
        public ActionResult Index()
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                WorkerDTO currentWorker = GetCurrentWorker(client);
                List<DefectCommentDTO> list = client.GetLastComments(currentWorker.Id, 20);
                ViewBag.UserId = currentWorker.Id;
                return View(list);
            }
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


                DefectDTO defect = client.ReportNewDefect(viewModel.Summary, viewModel.Description, viewModel.SelectedWorkOrderId, viewModel.AssigneeId, currentWorker.Id, viewModel.SelectedPriorityId, buffer, contentType);
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

        public ActionResult ListProblemsByUser(int? userId, string stateDescription)
        {
            ViewBag.EditMode = true;
            ViewBag.StateDescription = stateDescription;

            if (userId.HasValue)
                ViewBag.UserId = userId.Value;

            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<DefectDTO> result;

                if (userId.HasValue)
                    result = client.GetDefectsByWorker(userId.Value);
                else
                {
                    WorkerDTO currentWorker = GetCurrentWorker(client);
                    ViewBag.UserId = currentWorker.Id;
                    result = client.GetDefectsByWorker(currentWorker.Id);
                }

                result = ExtractResultByState(stateDescription, result);

                return View(result);
            }
        }

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

        public ActionResult ListProblemsByWorkOrder(int workOrderId, string stateDescription)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<DefectDTO> defects = client.GetAllDefectsByWorkOrder(workOrderId);
                List<WorkOrderDTO> workOrders = client.GetAllWorkOrders();

                defects = ExtractResultByState(stateDescription, defects);

                WorkOrderDTO selectedWorkOrder = workOrders.Where(w => w.Id == workOrderId).First();
                ViewBag.RadniNalog = selectedWorkOrder.Name;
                return View(defects);
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

                if (defect.DefectFollowers.Any(df => df.FollowerId == currentWorker.Id))
                    ViewBag.ShowEditBUtton = true;
                else
                    ViewBag.ShowEditBUtton = false;

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
                DefectViewModel viewModel = new DefectViewModel(defect);
                viewModel.Workers = client.GetActiveWorkers();
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Edit(DefectViewModel viewModel)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                WorkerDTO currentWorker = GetCurrentWorker(client);
                DefectDTO defect = client.SaveDefect(viewModel.Id, viewModel.Summary, viewModel.Description, viewModel.SelectedWorkOrderId, viewModel.AssigneeId, currentWorker.Id, viewModel.SelectedPriorityId, viewModel.StateDescription);

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
    }
}