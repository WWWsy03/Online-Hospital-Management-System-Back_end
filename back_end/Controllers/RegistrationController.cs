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

        [HttpGet("GetAllRegist")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRegist()
        {
            return await _context.Registrations.ToListAsync();
        }

        [HttpGet("GetFromDate")]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistFromDate(DateTime date)
        {
            var registrations = await _context.Registrations
                .Where(r => r.AppointmentTime.Date == date.Date)
                .Include(r => r.Patient) //使用Include方法来包含该导航属性获取病人的姓名
                .Include(r => r.Doctor)
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

        [HttpGet("GetFromDate&Period")]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistFromDatePeriod(DateTime date, decimal period)
        {
            var registrations = await _context.Registrations
                .Where(r => r.AppointmentTime.Date == date.Date)
                .Where(r => r.Period == period)
                .Include(r => r.Patient) //使用Include方法来包含该导航属性获取病人的姓名
                .Include(r => r.Doctor)
                .ToListAsync();

            var result = registrations
                .GroupBy(r => r.Period)  // 根据 Period 属性进行分组
                .Select(g => new
                {
                    Period = g.Key, // 这是每个分组的 Period 值
                    Count = g.Count(),
                    Patients = g.Select(r => new { Id = r.PatientId, Name = r.Patient.Name }).ToList()
                })
                .ToList();

            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }

        [HttpGet("Doctor/{ID}")]
        public IActionResult GetRegistFromDoctorId(string ID)
        {
            var registrations = _context.Registrations
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.DoctorId == ID)
                .ToList();

            return Ok(registrations);
        }

        [HttpGet("Patient/{ID}")]
        public async Task<IActionResult> GetRegistByPatientId(string ID)
        {
            var registrations = await _context.Registrations
                                        .Include(r => r.Doctor)
                                        .Include(r => r.Patient)
                                        .Where(r => r.PatientId == ID)
                                        .ToListAsync();

            if (!registrations.Any())
                return NotFound();

            var results = registrations.Select(reg =>
            {
                var queueCount = _context.Registrations
                                   .Where(r => r.AppointmentTime.Date == reg.AppointmentTime.Date &&
                                   r.Period == reg.Period &&
                                   r.DoctorId == reg.DoctorId&&
                                   r.Registorder < reg.Registorder)
                                   .Count();

                return new
                {
                    Doctor = reg.Doctor,
                    Patient = reg.Patient,
                    Date = reg.AppointmentTime.Date,
                    Period = reg.Period,
                    QueueCount = queueCount
                };
            }).ToList();

            return Ok(results);
        }


        [HttpPut("ReorderRegistByPatientId")]
        public IActionResult UpdateRegistOrder()
        {
            // 之前的功能代码：
            var registrations = _context.Registrations
                            .OrderBy(r => r.AppointmentTime)
                            .ThenBy(r => r.PatientId)
                            .ToList();

            var groupedRecords = registrations.GroupBy(r => new
            {
                Date = r.AppointmentTime.Date,
                r.Period,
                r.DoctorId
            }).ToList();

            foreach (var group in groupedRecords)
            {
                int order = 1;
                foreach (var record in group)
                {
                    record.Registorder = order;
                    order++;
                }
            }

            _context.SaveChanges();

            return Ok("Records updated successfully!");
        }

        [HttpPost("regist")]
        public async Task<ActionResult<Registration>> CreateRegistration([FromBody] RegistrationInputModel input)
        {
            // 获取当前最大的 Registorder 值
            var maxOrder = _context.Registrations
                .Where(r => r.DoctorId == input.DoctorId && r.AppointmentTime.Date == input.Time.Date && r.Period == input.Period)
                .Max(r => (int?)r.Registorder) ?? 0;

            var registration = new Registration
            {
                PatientId = input.PatientId,
                DoctorId = input.DoctorId,
                AppointmentTime = input.Time,
                Period = input.Period,
                Registorder = maxOrder + 1  // 设置 Registorder 为当前最大值加1
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
    }

    public class RegistrationInputModel//用于传输数据
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public DateTime Time { get; set; }
        public int Period { get; set; }
    }


}
