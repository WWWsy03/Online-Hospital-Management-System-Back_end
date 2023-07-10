using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Disease
    {
        public string DiseaseName { get; set; } = null!;
        public string? SecondaryDepartmentId { get; set; }
        public string Symptom { get; set; } = null!;
        public string? CureMethod { get; set; }

        public virtual Department2 DiseaseNameNavigation { get; set; } = null!;
    }
}
