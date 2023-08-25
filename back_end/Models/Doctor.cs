using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace back_end.Models
{
    public partial class Doctor
    {
        public Doctor()
        {
            ConsultationInfos = new HashSet<ConsultationInfo>();
            Prescriptions = new HashSet<Prescription>();
            Referrals = new HashSet<Referral>();
            Registrations = new HashSet<Registration>();
            TreatmentFeedbacks = new HashSet<TreatmentFeedback>();
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
        public string? Password { get; set; }
        [JsonIgnore]//防止序列化器尝试序列化这个集合，从而避免了循环引用的问题。
        public virtual ICollection<ConsultationInfo> ConsultationInfos { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; }
        public virtual ICollection<Referral> Referrals { get; set; }
        [JsonIgnore]//防止序列化器尝试序列化这个集合，从而避免了循环引用的问题。
        public virtual ICollection<Registration> Registrations { get; set; }
        public virtual ICollection<TreatmentFeedback> TreatmentFeedbacks { get; set; }
        public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; }
    }
}
