using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Subsequentvisit
    {
        public string Diagnosedid { get; set; } = null!;
        public string? Chattingrecords { get; set; }

        public virtual TreatmentRecord Diagnosed { get; set; } = null!;
    }
}
