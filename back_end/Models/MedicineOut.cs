﻿using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class MedicineOut
    {
        public string MedicineName { get; set; } = null!;
        public string Manufacturer { get; set; } = null!;
        public DateTime ProductionDate { get; set; }
        public decimal PurchaseAmount { get; set; }
        public DateTime DeliverDate { get; set; }
        public string PatientId { get; set; } = null!;

        public virtual Patient Patient { get; set; } = null!;
    }
}
