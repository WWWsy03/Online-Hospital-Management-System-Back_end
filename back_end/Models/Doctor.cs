using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        [JsonIgnore]//防止序列化器尝试序列化这个集合，从而避免了循环引用的问题。
        public virtual ICollection<ConsultationInfo> ConsultationInfos { get; set; }
        [JsonIgnore]//防止序列化器尝试序列化这个集合，从而避免了循环引用的问题。
        public virtual ICollection<Prescription> Prescriptions { get; set; }
        [JsonIgnore]//防止序列化器尝试序列化这个集合，从而避免了循环引用的问题。
        public virtual ICollection<Registration> Registrations { get; set; }
        public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; }
    }
}
