using System.Text.Json;
using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PersoninfoController : ControllerBase
    {
        private readonly ModelContext _context;
        public PersoninfoController(ModelContext context) {
            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistrations(string PatientID)//查询每个给定日期各个时间段都有多少人挂号，学号是多少
        {
            var result = await _context.Patients.Where(r => r.PatientId == PatientID).ToListAsync();

            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }
    }
}
