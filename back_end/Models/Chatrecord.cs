using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Chatrecord
    {
        public string? Id { get; set; }
        public string? DoctorId { get; set; }
        public string? PatientId { get; set; }
        public string? Message { get; set; }
        public string? Column4 { get; set; }
        public DateTime? Column5 { get; set; }
        public string? ReadStatus { get; set; }
    }
}
