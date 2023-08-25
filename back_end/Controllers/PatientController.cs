using back_end.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : Controller
    {
        private readonly ModelContext _context;
        public PatientController(ModelContext context)
        {
            _context = context;
        }

        //插入医生信息
        [HttpPost("PostTreatmentRord1")]
        public async Task<ActionResult<TreatmentRecord>> PostTreatmentRecord(TreatmentRecord row)
        {
            _context.TreatmentRecords.Add(row);
            await _context.SaveChangesAsync();

            return CreatedAtAction("PostTreatmentRecord", new { id = row.DoctorId }, row);
        }

        [HttpGet("GetAllTreatmentRord1")]
        public async Task<ActionResult<IEnumerable<TreatmentRecord>>> GetTreatmentRecords()
        {
            return await _context.TreatmentRecords.ToListAsync();
        }

        [HttpGet("PatientDetails/{patientId}")]
        public async Task<ActionResult<object>> GetPatientDetails(string patientId)
        {
            var result = await _context.Patients
                .Where(p => p.PatientId == patientId) // 添加这一行来过滤结果
                .Join(
                    _context.TreatmentRecords,
                    patient => patient.PatientId,
                    record1 => record1.PatientId,
                    (patient, record1) => new { patient, record1 }
                )
                .Join(
                    _context.TreatmentRecord2s,
                    combined => combined.record1.DiagnosisRecordId,
                    record2 => record2.DiagnoseId,
                    (combined, record2) => new
                    {
                        PatientId = combined.patient.PatientId,
                        Name = combined.patient.Name,
                        DiagnosisTime = record2.DiagnoseTime
                    }
                )
                .FirstOrDefaultAsync(); // 改为FirstOrDefaultAsync，因为我们只寻找一个特定的病人的详情

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }



        [HttpGet("WenhaoYan_test")]
        public IActionResult Query()
        {
            WenhaoYan_model ans = new WenhaoYan_model
            {
                Date = DateTime.Now,
                department="内科",
                status="待就诊",
                appointmentNumber="123456",
                doctor="张医生",
                appointmentTime="上午",
                waitingCount=5
            };

            return Ok(ans);
        }


        //[HttpPut("update-passwords")]
        //public async Task<IActionResult> UpdateAllDoctorPasswords()
        //{
        //    // 获取所有的Doctor记录
        //    var doctors = _context.Doctors.ToList();
        //    var administs = _context.Administrators.ToList();
        //    var patients = _context.Patients.ToList();

        //    // 遍历所有医生并为他们设置新的随机密码
        //    foreach (var doctor in doctors)
        //    {
        //        doctor.Password = GenerateRandomPassword(6);
        //    }
        //    foreach (var administ in administs)
        //    {
        //        administ.Password = GenerateRandomPassword(6);
        //    }
        //    foreach (var patient in patients)
        //    {
        //        patient.Password = GenerateRandomPassword(6);
        //    }

        //    // 保存更改
        //    await _context.SaveChangesAsync();

        //    return Ok("Passwords updated successfully.");
        //}

        //// 生成随机密码的辅助方法
        //private string GenerateRandomPassword(int length)
        //{
        //    const string valid = "1234567890";
        //    var res = new char[length];
        //    var rng = new Random();
        //    for (int i = 0; i < length; i++)
        //    {
        //        res[i] = valid[rng.Next(valid.Length)];
        //    }
        //    return new string(res);
        //}
    }
}
