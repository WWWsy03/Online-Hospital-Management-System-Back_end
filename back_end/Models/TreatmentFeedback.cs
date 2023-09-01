using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class TreatmentFeedback
    {
        public string PatientId { get; set; } = null!;
        public string DoctorId { get; set; } = null!;
        public decimal? TreatmentScore { get; set; }
        public string? Evaluation { get; set; }
        public string? FollowUpMatters { get; set; }
        public string Diagnosedid { get; set; } = null!;

        public virtual Doctor Doctor { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
    }
}
