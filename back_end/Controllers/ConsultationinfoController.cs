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
    
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetConsultationInfoByDepartmentAndKeyword(string department, string keyword)
        {
            var doctors = await _context.Doctors
                .Where(d => d.SecondaryDepartment == department && d.Name.Contains(keyword))
                .ToListAsync();

            if (doctors == null || doctors.Count == 0)
            {
                return NotFound();
            }

            var result = new List<object>();

            foreach (var doctor in doctors)
            {
                var consultationInfos = await _context.ConsultationInfos
                    .Where(c => c.DoctorId == doctor.DoctorId)
                    .Include(c => c.ClinicNameNavigation) // 使用Include方法来包含该导航属性获取诊室的名称
                    .ToListAsync();

                var doctorResult = consultationInfos.GroupBy(c => c.DoctorId) // 按照医生ID分组
                    .Select(g => new
                    {
                        DoctorId = g.Key,
                        DoctorName = doctor.Name, // 添加医生姓名
                        Count = g.Count(),
                        ConsultationInfos = g.Select(c => new { ClinicName = c.ClinicName, DateTime = c.DateTime, Period = c.Period }).ToList() // Select the clinic's name and consultation times
                    })
                    .OrderBy(r => r.DoctorId)
                    .ToList();

                result.AddRange(doctorResult);
            }

            return result;
        }


    }
}
