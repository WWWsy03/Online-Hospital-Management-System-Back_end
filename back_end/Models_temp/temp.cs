namespace back_end
{
    public class Instructor
    {
        public string? name { get; set; }
        public string? description { get; set; }
        public string? img_link { get; set; }
    }
    public class WenhaoYan_model
    {
        public DateTime Date { get; set; }
        public string? department { get; set; }
        public string? status { get; set; }
        public string? appointmentNumber { get; set; }
        public string? doctor { get; set; }
        public string? appointmentTime { get; set; }
        public int waitingCount { get; set; }
    }
}
