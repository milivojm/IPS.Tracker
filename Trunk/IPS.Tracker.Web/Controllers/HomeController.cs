using System;
using System.Collections.Generic;
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
                ViewBag.UserId = currentWorker.Id;
            }

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

                DefectDTO defect = client.ReportNewDefect(viewModel.Summary, viewModel.Description, viewModel.SelectedWorkOrderId, viewModel.AssigneeId, currentWorker.Id, viewModel.SelectedPriorityId, null);
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

        public ActionResult ListProblemsByUser(int? userId)
        {
            ViewBag.EditMode = true;

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

                ViewBag.NumberOfPages = Math.Ceiling((double)result.Count() / defectsPerPage);
                ViewBag.DefectsPerPage = defectsPerPage;
                return View(result);
            }
        }

        public ActionResult ListProblemsByWorkOrder(int workOrderId)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                List<DefectDTO> defects = client.GetDefectsByWorkOrder(workOrderId);
                List<WorkOrderDTO> workOrders = client.GetActiveWorkOrders();
                WorkOrderDTO selectedWorkOrder = workOrders.Where(w => w.Id == workOrderId).First();
                ViewBag.RadniNalog = selectedWorkOrder.Name;
                ViewBag.EditMode = false;
                ViewBag.NumberOfPages = Math.Ceiling((double)defects.Count() / defectsPerPage);
                ViewBag.DefectsPerPage = defectsPerPage;
                return View(defects);
            }
        }

        public ActionResult Details(int id)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                DefectDTO defect = client.GetDefectById(id);

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
                viewModel = new DefectViewModel(defect);
                viewModel.Workers = client.GetActiveWorkers();
                return View(viewModel);
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