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
        public async Task<ActionResult<IEnumerable<object>>> GetRegistrations(string PatientID)//给定学号查询个人信息
        {
            var result = await _context.Patients.Where(r => r.PatientId == PatientID).ToListAsync();

            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }
        [HttpPut("update")]
        public async Task<IActionResult> UpdatePatient(Models.Patient patient)
        {
            if (!PatientExists(patient.PatientId))//先检查一下要修改的信息存不存在
            {
                return NotFound();
            }

            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();//实现修改
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(patient.PatientId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        private bool PatientExists(string id)
        {
            return _context.Patients.Any(e => e.PatientId == id);
        }


    }
}
