using back_end.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Patient : Controller
    {
        private readonly ModelContext _context;
        public Patient(ModelContext context)
        {
            _context = context;
        }

        [HttpGet("AllPatients")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllPatients()
        {
            var administrators = await _context.Patients
                .Select(a => new { a.PatientId, a.Name, a.Gender, a.BirthDate,a.Contact,a.Password,a.College,a.Counsellor })
                .ToListAsync();

            if (!administrators.Any())
            {
                return NotFound();
            }

            return Ok(administrators);
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
    }
}
