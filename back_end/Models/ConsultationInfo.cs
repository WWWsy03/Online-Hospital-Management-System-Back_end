using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class ConsultationInfo
    {
        public string DoctorId { get; set; } = null!;
        public string ClinicName { get; set; } = null!;
        public DateTime DateTime { get; set; }
        public decimal Period { get; set; }

        public virtual ConsultingRoom ClinicNameNavigation { get; set; } = null!;
        public virtual Doctor Doctor { get; set; } = null!;
    }
}
