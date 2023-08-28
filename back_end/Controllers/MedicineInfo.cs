using back_end.Models;

namespace back_end.Controllers
{
    internal class MedicineInfo
    {
        public string MedicineName { get; set; }
        public string MedicineType { get; set; }
        public string ApplicableSymptom { get; set; }
        public string Vulgo { get; set; }
        public string Specification { get; set; }
        public string Singledose { get; set; }
        public string Administration { get; set; }
        public string Attention { get; set; }
        public string Frequency { get; set; }
        public ICollection<PrescriptionMedicine> PrescriptionMedicines { get; set; }
        public decimal SellingPrice { get; set; }
    }
}