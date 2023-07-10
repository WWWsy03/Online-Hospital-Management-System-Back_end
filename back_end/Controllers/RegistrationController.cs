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
            var registrations = await _context.Registrations.Where(r => r.AppointmentTime.Date == date.Date).ToListAsync();

            var result = registrations.GroupBy(r => r.Period)
              .Select(g => new { Period = g.Key, Count = g.Count(), PatientIds = g.Select(r => r.PatientId).ToList() })
              .OrderBy(r => r.Period)
              .ToList();


            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }
    }

}
