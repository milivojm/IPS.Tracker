using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IPS.Tracker.Domain
{
    public class Worker
    {
        public static List<Worker> AllWorkers { get; set; }

        public int Id { get; private set; }

        public string Name { get; private set; }

        public string Retired { get; private set; }

        public string Username { get; private set; }

        public string TrackerAdmin { get; private set; }

        public string Email { get; private set; }
    }
}
