using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class TreatmentRecord
    {
        public string DiagnosisRecordId { get; set; } = null!;
        public string? DoctorId { get; set; }
        public string? PatientId { get; set; }
        public string? LeaveNoteId { get; set; }

        public virtual Doctor? Doctor { get; set; }
        public virtual LeaveApplication? LeaveNote { get; set; }
        public virtual Patient? Patient { get; set; }
        public virtual TreatmentFeedback? TreatmentFeedback { get; set; }
        public virtual TreatmentRecord2? TreatmentRecord2 { get; set; }
    }
}
