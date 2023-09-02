using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Registration
    {
        public string PatientId { get; set; } = null!;
        public string DoctorId { get; set; } = null!;
        public DateTime AppointmentTime { get; set; }
        public decimal Period { get; set; }
        public decimal? Registorder { get; set; }
        public decimal State { get; set; }
        public string? Prescriptionid { get; set; }
        public decimal Checkin { get; set; }

        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
        public virtual Prescription? Prescription { get; set; }
    }
}
