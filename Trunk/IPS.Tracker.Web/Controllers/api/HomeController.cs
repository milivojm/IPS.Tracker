using IPS.Tracker.Web.TrackerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IPS.Tracker.Web.Controllers.api
{
    public class HomeController : ApiController
    {
        public List<int> GetId(int numberOfRecords)
        {
            List<int> result = new List<int>();

            for (int i = 1; i <= numberOfRecords; i++)
                result.Add(i);

            return result;
        }

        public List<DefectCommentDTO> GetLastComments(int numberOfRecords)
        {
            using (TrackerServiceClient client = new TrackerServiceClient())
            {
                WorkerDTO currentWorker = GetCurrentWorker(client);
                List<DefectCommentDTO> list = client.GetLastComments(numberOfRecords);
                return list;
            }
        }

        private WorkerDTO GetCurrentWorker(TrackerServiceClient client)
        {
            string currentUsername = User.Identity.Name.Split('\\')[1];
            List<WorkerDTO> workers = client.GetActiveWorkers();
            WorkerDTO currentWorker = workers.Where(w => w.Username.ToUpper() == currentUsername.ToUpper()).First();
            return currentWorker;
        }
    }
}
