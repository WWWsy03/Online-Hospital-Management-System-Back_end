using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class PrescriptionMedicine
    {
        public string PrescriptionId { get; set; } = null!;
        public string MedicineName { get; set; } = null!;
        public string MedicationInstruction { get; set; } = null!;
        public decimal MedicinePrice { get; set; }
        public decimal Quantity { get; set; }

        public virtual MedicineDescription MedicineNameNavigation { get; set; } = null!;
    }
}
