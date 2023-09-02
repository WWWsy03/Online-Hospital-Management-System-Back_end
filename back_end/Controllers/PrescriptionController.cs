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
        public async Task<IActionResult> GetPrescriptionById(string diagnoseId)
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
                    MedicinePrice = m.MedicinePrice,
                    Quantity=m.Quantity
                })
            };

            return Ok(result);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllPrescription()
        {
            var prescriptions = _context.Prescriptions.ToList();

            var result = prescriptions.Select(p =>
            {
                var diagnoseId = p.PrescriptionId.Remove(8, 3);
                var diagnose = _context.TreatmentRecords.FirstOrDefault(d => d.DiagnosisRecordId == diagnoseId);
                var treatmentRecord = _context.TreatmentRecord2s.FirstOrDefault(d => d.DiagnoseId == diagnoseId);
                return new
                {
                    PrescriptionId = p.PrescriptionId,
                    DiagnoseTime = treatmentRecord.DiagnoseTime,
                    PatientId = diagnose.PatientId,
                    TotalPrice = p.TotalPrice,
                    Paystate = p.Paystate,
                    DoctorId = p.DoctorId
                };
            });

            return Ok(result);
        }


        [HttpGet("GetDetail")]
        public async Task<IActionResult> GetDetail(string prescriptionId)
        {
            var prescriptionMedicines = await _context.PrescriptionMedicines.Where(p => p.PrescriptionId == prescriptionId).ToListAsync();
            if (prescriptionMedicines.Count == 0)
            {
                return NotFound();
            }

            var result = prescriptionMedicines.Select(pm => new
            {
                MedicineName = pm.MedicineName,
                MedicationInstruction = pm.MedicationInstruction,
                MedicinePrice = pm.MedicinePrice,
                Quantity = pm.Quantity
            });

            return Ok(result);
        }

        [HttpPut("UpdatePaystate")]//订单支付
        public async Task<IActionResult> UpdatePaystate(string prescriptionId)
        {
            var prescription = await _context.Prescriptions.FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);
            if (prescription == null)
            {
                return NotFound();
            }
            if(prescription.Paystate == 1)
            {
                return BadRequest("该订单已支付");
            }
            prescription.Paystate = 1;
            await _context.SaveChangesAsync();

            return Ok("支付成功");
        }

    }
}
