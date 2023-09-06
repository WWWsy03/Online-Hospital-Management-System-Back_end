using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace back_end.Models
{
    public partial class MedicineStock
    {
        public string MedicineName { get; set; } = null!;
        public string Manufacturer { get; set; } = null!;
        public DateTime ProductionDate { get; set; }
        public decimal MedicineShelflife { get; set; }
        public decimal? MedicineAmount { get; set; }
        public decimal ThresholdValue { get; set; }
        public DateTime? CleanDate { get; set; }
        public string? CleanAdministrator { get; set; }
        [JsonIgnore]//防止序列化器尝试序列化这个集合，从而避免了循环引用的问题。
        public virtual Administrator? CleanAdministratorNavigation { get; set; }
    }
}
