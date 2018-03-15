using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace IPS.Tracker.WCF
{
    [DataContract]
    public class ReleaseDTO
    {
        [DataMember]
        public string ReleaseVersion { get; set; }
        [DataMember]
        public DateTime ReleaseDate { get; set; }
    }
}