using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosedHistoryController : ControllerBase
    {
        private readonly ModelContext _context;

        public DiagnosedHistoryController(ModelContext context)
        {
            _context = context;
        }
        [HttpGet("getPatientRecords")]
        public async Task<ActionResult<IEnumerable<TreatmentRecord2>>> GetPatientRecords(string patientId)
        {
            // 查找与病人ID匹配的所有诊断记录ID
            var diagnosisRecordIds = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId)
                .Select(r => r.DiagnosisRecordId)
                .ToListAsync();

            // 使用这些诊断记录ID在TreatmentRecord2表中查询每个ID对应的属性
            var patientRecords = await _context.TreatmentRecord2s
                .Where(r => diagnosisRecordIds.Contains(r.DiagnoseId))
                .ToListAsync();

            return Ok(patientRecords);
        }


        [HttpGet("GetDetailPre")]
        public async Task<IActionResult> GetDetailPre(string patientId)
        {
            // 查找与病人ID匹配的所有诊断记录ID
            var diagnosisRecordIds = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId)
                .Select(r => r.DiagnosisRecordId)
                .ToListAsync();

            // 使用这些诊断记录ID在TreatmentRecord2表中查询每个ID对应的属性
            var patientRecords = await _context.TreatmentRecord2s
                .Where(r => diagnosisRecordIds.Contains(r.DiagnoseId))
                .ToListAsync();

            // 根据病人ID查询病人的个人信息
            var patientInfo = await _context.Patients
                .Where(p => p.PatientId == patientId)
                .Select(p => new
                {
                    p.PatientId,
                    p.Name,
                    p.Gender,
                    p.BirthDate,
                    p.Contact,
                    p.Password,
                    p.College,
                    p.Counsellor
                })
                .FirstOrDefaultAsync();

            if (patientInfo == null)
            {
                return NotFound("No patient found with this patientId.");
            }

            // 根据找到的diagnoseId，将其第八位之后加“000”在拼接其后半部分得到处方ID
            var prescriptionIds = diagnosisRecordIds
                .Select(id => id.Substring(0, 8) + "000" + id.Substring(8))
                .ToList();

            // 在PrescriptionMedicine表中查询每个处方ID对应的药品名称和数量
            var medicines = await _context.PrescriptionMedicines
                .Where(m => prescriptionIds.Contains(m.PrescriptionId))
                .Select(m => new { m.PrescriptionId, m.MedicineName, m.Quantity })
                .ToListAsync();

            // 使用找到的药品名称在MedicineDescription表中查询每种药品对应的属性
            var medicineDescriptions = await _context.MedicineDescriptions
                .Where(m => medicines.Select(x => x.MedicineName).Contains(m.MedicineName))
                .Select(m => new
                {
                    m.MedicineName,
                    m.Specification,
                    m.Singledose,
                    m.Administration,
                    m.Attention,
                    m.Frequency
                })
                .ToListAsync();
            var doctorIds = patientRecords.Select(r => r.DiagnoseId.Substring(15, 5)).ToList();

            // 在医生表中查询每个医生ID对应的信息
            var doctors = await _context.Doctors
                .Where(d => doctorIds.Contains(d.DoctorId))
                .Select(d => new { d.DoctorId, d.Name, d.Title, d.SecondaryDepartment })
                .ToListAsync();
            // 将每次诊断记录及其对应的处方和药品信息分组返回
            var records = patientRecords.Select(r => new
            {
                Record = r,
                Prescription = medicines.Where(m => m.PrescriptionId == r.DiagnoseId.Substring(0, 8) + "000" + r.DiagnoseId.Substring(8)).ToList(),
                MedicineDescriptions = medicineDescriptions.Where(m => medicines.Where(x => x.PrescriptionId == r.DiagnoseId.Substring(0, 8) + "000" + r.DiagnoseId.Substring(8)).Select(x => x.MedicineName).Contains(m.MedicineName)).ToList(),
                Doctor = doctors.FirstOrDefault(d => d.DoctorId == r.DiagnoseId.Substring(15, 5))
        }).ToList();

            return Ok(new { patientInfo, records });
        }


    }
}
