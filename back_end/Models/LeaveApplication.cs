﻿using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class LeaveApplication
    {
        public string LeaveNoteId { get; set; } = null!;
        public DateTime LeaveApplicationTime { get; set; }
        public DateTime LeaveStartDate { get; set; }
        public DateTime LeaveEndDate { get; set; }
        public string? LeaveNoteRemark { get; set; }
    }
}
