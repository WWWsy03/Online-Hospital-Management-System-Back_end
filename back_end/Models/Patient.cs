using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Patient
    {
        public Patient()
        {
            Chatrecords = new HashSet<Chatrecord>();
            MedicineOuts = new HashSet<MedicineOut>();
            OutpatientOrders = new HashSet<OutpatientOrder>();
            Registrations = new HashSet<Registration>();
            TreatmentRecords = new HashSet<TreatmentRecord>();
        }

        public string PatientId { get; set; } = null!;
        public string? Name { get; set; }
        public bool? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Contact { get; set; }
        public string Password { get; set; } = null!;
        public string College { get; set; } = null!;
        public string Counsellor { get; set; } = null!;

        public virtual ICollection<Chatrecord> Chatrecords { get; set; }
        public virtual ICollection<MedicineOut> MedicineOuts { get; set; }
        public virtual ICollection<OutpatientOrder> OutpatientOrders { get; set; }
        public virtual ICollection<Registration> Registrations { get; set; }
        public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; }
    }
}
