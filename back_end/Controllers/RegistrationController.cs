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
        public async Task<ActionResult<IEnumerable<object>>> GetRegistrations(DateTime date)//查询每个给定日期各个时间段都有多少人挂号，学号是多少
        {
            var registrations = await _context.Registrations.Where(r => r.AppointmentTime.Date == date.Date).ToListAsync();

            var result = registrations.GroupBy(r => r.Period)
              .Select(g => new { Period = g.Key, Count = g.Count(), PatientIds = g.Select(r => r.PatientId).ToList() })
              .OrderBy(r => r.Period)
              .ToList();


            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }
        [HttpPost]
        public async Task<ActionResult<Registration>> CreateRegistration(JsonElement data)
        {
            string patientId = data.GetProperty("PatientId").GetString();
            string doctorId = data.GetProperty("DoctorId").GetString();
            var registration = new Registration
            {
                PatientId = data.GetProperty("PatientId").GetString(),
                DoctorId = data.GetProperty("DoctorId").GetString(),
                AppointmentTime = data.GetProperty("AppointmentTime").GetDateTime(),
                Period = data.GetProperty("Period").GetDecimal()
            };

            registration.Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
            registration.Patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            return Ok(registration);
        }


    }

}
