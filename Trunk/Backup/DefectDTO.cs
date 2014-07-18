﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace IPS.Tracker.WCF
{
    [DataContract]
    public class DefectDTO
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public short Priority { get; set; }
        [DataMember]
        public string Summary { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public int? WorkOrderId { get; set; }
        [DataMember]
        public int ReporterId { get; set; }
        [DataMember]
        public int AssigneeId { get; set; }
        [DataMember]
        public Byte[] DefectFile { get; set; }
        [DataMember]
        public DateTime DefectDate { get; set; }
        [DataMember]
        public string DefectState { get; set; }
    }
}