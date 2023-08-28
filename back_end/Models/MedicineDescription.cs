using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class MedicineDescription
    {
        public MedicineDescription()
        {
            PrescriptionMedicines = new HashSet<PrescriptionMedicine>();
        }

        public string MedicineName { get; set; } = null!;
        public string MedicineType { get; set; } = null!;
        public string? ApplicableSymptom { get; set; }
        public string? Vulgo { get; set; }
        public string? Specification { get; set; }
        public string? Singledose { get; set; }
        public string? Administration { get; set; }
        public string? Attention { get; set; }
        public string? Frequency { get; set; }

        public virtual ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; }
    }
}
