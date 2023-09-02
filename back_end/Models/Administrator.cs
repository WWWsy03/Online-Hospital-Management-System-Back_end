using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Administrator
    {
        public Administrator()
        {
            MedicinePurchases = new HashSet<MedicinePurchase>();
            MedicineStocks = new HashSet<MedicineStock>();
        }

        public string AdministratorId { get; set; } = null!;
        public string? Name { get; set; }
        public bool? Gender { get; set; }
        public DateTime? Birthdate { get; set; }
        public string? Contact { get; set; }
        public string Password { get; set; } = null!;

        public virtual ICollection<MedicinePurchase> MedicinePurchases { get; set; }
        public virtual ICollection<MedicineStock> MedicineStocks { get; set; }
    }
}
