using System.Text.Json;
using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly ModelContext _context;

        public RegistrationController(ModelContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistrations(DateTime date)
        {
            var registrations = await _context.Registrations
                .Where(r => r.AppointmentTime.Date == date.Date)
                .Include(r => r.Patient) //使用Include方法来包含该导航属性获取病人的姓名
                .ToListAsync();

            var result = registrations.GroupBy(r => r.Period)//按照挂号时间分组
                .Select(g => new
                {
                    Period = g.Key,
                    Count = g.Count(),
                    Patients = g.Select(r => new { Id = r.PatientId, Name = r.Patient.Name }).ToList() // Select the patient's ID and name
                })
                .OrderBy(r => r.Period)
                .ToList();

            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }

        [HttpPost("regist")]
        public async Task<ActionResult<Registration>> CreateRegistration([FromBody] RegistrationInputModel input) { 
            var registration = new Registration
            {
                PatientId = input.PatientId,
                DoctorId = input.DoctorId,
                AppointmentTime = input.Time,
                Period = input.Period
            };

            registration.Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == input.DoctorId);
            registration.Patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == input.PatientId);

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(registration);
        }

        [HttpDelete("cancel")]
        public async Task<IActionResult> DeleteRegistration([FromBody] RegistrationInputModel inputModel)
        {

            // 查找匹配的挂号记录
            var registration = await _context.Registrations.FirstOrDefaultAsync(r =>
                r.PatientId == inputModel.PatientId &&
                r.DoctorId == inputModel.DoctorId &&
                r.AppointmentTime.Date == inputModel.Time.Date &&
                r.Period == inputModel.Period);

            // 如果找不到匹配的挂号记录，返回错误信息
            if (registration == null)
            {
                return NotFound("No registration found.");
            }

            // 从数据库中删除找到的挂号记录
            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("successful.");
        }


        [HttpGet("commit")]
        public IActionResult GetRegistrationsByDoctorId(string doctorId)
        {
            var currentDate = DateTime.Now.Date;
            var registrations = _context.Registrations
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId && r.AppointmentTime.Date == currentDate)
                .ToList();

            return Ok(registrations);
        }

    }

    public class RegistrationInputModel//用于传输数据
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public DateTime Time { get; set; }
        public int Period { get; set; }
    }


}
