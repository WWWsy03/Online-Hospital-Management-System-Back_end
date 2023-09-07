using System;
using System.Collections.Generic;

namespace back_end.Models
{
    public partial class ConsultingRoom
    {
        public ConsultingRoom()
        {
            ConsultationInfos = new HashSet<ConsultationInfo>();
        }

        public string ConsultingRoomName { get; set; } = null!;
        public decimal? ConsultantCapacity { get; set; }

        public virtual ICollection<ConsultationInfo> ConsultationInfos { get; set; }
    }
}
