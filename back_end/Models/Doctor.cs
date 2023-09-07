using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Doctor
    {
        public Doctor()
        {
            Chatrecords = new HashSet<Chatrecord>();
            ConsultationInfos = new HashSet<ConsultationInfo>();
            Prescriptions = new HashSet<Prescription>();
            Registrations = new HashSet<Registration>();
            TreatmentRecords = new HashSet<TreatmentRecord>();
        }

        public string DoctorId { get; set; } = null!;
        public string? Name { get; set; }
        public bool? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Title { get; set; }
        public string? Contact { get; set; }
        public string? SecondaryDepartment { get; set; }
        public string? Photourl { get; set; }
        public string Password { get; set; } = null!;
        public string? Skilledin { get; set; }

        public virtual ICollection<Chatrecord> Chatrecords { get; set; }
        public virtual ICollection<ConsultationInfo> ConsultationInfos { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
        public virtual ICollection<Registration> Registrations { get; set; }
        public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; }
    }
}
