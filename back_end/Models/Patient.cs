﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace back_end.Models
{
    public partial class Patient
    {
        public Patient()
        {
            MedicineOuts = new HashSet<MedicineOut>();
            OutpatientOrders = new HashSet<OutpatientOrder>();
            Referrals = new HashSet<Referral>();
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

        public virtual ICollection<MedicineOut> MedicineOuts { get; set; }
        public virtual ICollection<OutpatientOrder> OutpatientOrders { get; set; }
        public virtual ICollection<Referral> Referrals { get; set; }
        [JsonIgnore]//防止序列化器尝试序列化这个集合，从而避免了循环引用的问题。
        public virtual ICollection<Registration> Registrations { get; set; }
        public virtual ICollection<TreatmentRecord> TreatmentRecords { get; set; }
    }
}