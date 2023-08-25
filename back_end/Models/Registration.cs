using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Registration
    {
        public string PatientId { get; set; } = null!;
        public string DoctorId { get; set; } = null!;
        public DateTime AppointmentTime { get; set; }
        public decimal? Period { get; set; }
        public string? Registorder { get; set; }

        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
    }
}
