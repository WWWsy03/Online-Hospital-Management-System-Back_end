using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class TreatmentRecord2
    {
        public string DiagnoseId { get; set; } = null!;
        public DateTime? DiagnoseTime { get; set; }
        public decimal? Commentstate { get; set; }
        public string? Selfreported { get; set; }
        public string? Presenthis { get; set; }
        public string? Anamnesis { get; set; }
        public string? Sign { get; set; }
        public string? Clinicdia { get; set; }
        public string? Advice { get; set; }
        public decimal? Kindquantity { get; set; }

        public virtual TreatmentRecord Diagnose { get; set; } = null!;
    }
}
