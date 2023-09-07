using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Template
    {
        public string? Problem { get; set; }
        public string? Illness { get; set; }
        public string? Column1 { get; set; }
        public string? Symptom { get; set; }
        public string? Diagnose { get; set; }
        public string? Prescription { get; set; }
        public string? Medicine { get; set; }
        public string Name { get; set; } = null!;
    }
}
