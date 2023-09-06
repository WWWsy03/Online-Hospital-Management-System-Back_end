using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Chatrecord
    {
        public string Recordid { get; set; } = null!;
        public string DoctorId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string Message { get; set; } = null!;
        public decimal SenderType { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal ReadStatus { get; set; }

        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
    }
}
