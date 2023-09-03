using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Prescription
    {
        public Prescription()
        {
            Registrations = new HashSet<Registration>();
        }

        public string PrescriptionId { get; set; } = null!;
        public decimal TotalPrice { get; set; }
        public string DoctorId { get; set; } = null!;
        public decimal Paystate { get; set; }

        public virtual Doctor Doctor { get; set; } = null!;
        public virtual ICollection<Registration> Registrations { get; set; }
    }
}
