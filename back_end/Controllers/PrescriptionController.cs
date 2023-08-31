using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrescriptionController : ControllerBase
    {
        private readonly ModelContext _context;
        //private static readonly object _lock = new object();

        public PrescriptionController(ModelContext context)
        {
            _context = context;
        }

        [HttpGet("GetPrescription")]
        public async Task<IActionResult> GetDiagnoseAsync(string diagnoseId)
        {
            var prescriptionId = diagnoseId.Substring(0, 8) + "000" + diagnoseId.Substring(8);
            var prescription = await _context.Prescriptions.FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);
            if (prescription == null)
            {
                return NotFound();
            }

            var diagnose = await _context.TreatmentRecord2s.FirstOrDefaultAsync(d => d.DiagnoseId == diagnoseId);
            if (diagnose == null)
            {
                return NotFound();
            }

            var medicines = await _context.PrescriptionMedicines.Where(pm => pm.PrescriptionId == prescriptionId).ToListAsync();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == prescription.DoctorId);

            var result = new
            {
                TotalPrice = prescription.TotalPrice,
                Doctor = new
                {
                    Name = doctor.Name,
                    Title = doctor.Title,
                    SecondaryDepartment = doctor.SecondaryDepartment
                },
                DiagnoseTime = diagnose.DiagnoseTime,
                Anamnesis = diagnose.Anamnesis,
                Sign = diagnose.Sign,
                Clinicdia = diagnose.Clinicdia,
                Advice = diagnose.Advice,
                Medicines = medicines.Select(m => new
                {
                    MedicineName = m.MedicineName,
                    MedicationInstruction = m.MedicationInstruction,
                    MedicinePrice = m.MedicinePrice
                })
            };

            return Ok(result);
        }
    }
}
