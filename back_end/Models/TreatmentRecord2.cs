using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class TreatmentRecord2
    {
        public string DiagnoseId { get; set; } = null!;
        public DateTime? DiagnoseTime { get; set; }
        public decimal? Commentstate { get; set; }

        public virtual TreatmentRecord Diagnose { get; set; } = null!;
    }
}
