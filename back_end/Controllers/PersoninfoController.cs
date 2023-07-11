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
        public async Task<IActionResult> UpdatePatient(string id, Patient patient)//给定学号和要修改的信息修改个人信息
        {
            if (id != patient.PatientId){//首先检查传入的病人ID是否与Patient对象中的ID匹配
                return BadRequest();
            }

            _context.Entry(patient).State = EntityState.Modified;//标记已修改

            try{
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)//如果在保存更改时发生了并发冲突（例如，另一个用户同时修改了同一条记录）
                                                //，那么EF Core会抛出一个DbUpdateConcurrencyException异常。
            {
                if (!PatientExists(id))
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
