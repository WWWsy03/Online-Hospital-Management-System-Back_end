using System.Linq;
using Microsoft.AspNetCore.Mvc;
using back_end.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConsultationinfoController : ControllerBase
    {
        private readonly ModelContext _context;

        public ConsultationinfoController(ModelContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<ConsultationInfo>>> GetConsultations(string id)
        {
            var consultations = await _context.ConsultationInfos
                .Where(c => c.DoctorId == id)
                .ToListAsync();

            if (consultations == null)
            {
                return NotFound();
            }

            return consultations;
        }
        // 通过医生姓名查找其坐诊时间
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetConsultationInfoByName(string name)
        {
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Name == name);

            if (doctor == null)
            {
                return NotFound();
            }

            var consultationInfos = await _context.ConsultationInfos
                .Where(c => c.DoctorId == doctor.DoctorId)
                .Include(c => c.ClinicNameNavigation) // 使用Include方法来包含该导航属性获取诊室的名称
                .ToListAsync();

            var result = consultationInfos.GroupBy(c => c.DoctorId) // 按照医生ID分组
                .Select(g => new
                {
                    DoctorId = g.Key,
                    Count = g.Count(),
                    ConsultationInfos = g.Select(c => new { ClinicName = c.ClinicName, StartTime = c.StartTime, EndTime = c.EndTime }).ToList() // Select the clinic's name and consultation times
                })
                .OrderBy(r => r.DoctorId)
                .ToList();

            return result;
        }
    }
}
