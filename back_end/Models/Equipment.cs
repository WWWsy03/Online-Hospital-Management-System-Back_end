using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class Equipment
    {
        public string ConsultingRoomName { get; set; } = null!;
        public string EquipmentType { get; set; } = null!;
        public decimal? EquipmentAmount { get; set; }

        public virtual ConsultingRoom ConsultingRoomNameNavigation { get; set; } = null!;
    }
}
