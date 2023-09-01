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
        public async Task<ActionResult<IEnumerable<object>>> GetConsultationInfoByDepartmentAndKeyword(string department, string? keyword)//加上问号表示这个参数可以为空
        {
            var doctors = keyword != null
                ? await _context.Doctors
                    .Where(d => d.SecondaryDepartment == department && d.Name.Contains(keyword))
                    .ToListAsync()
                : await _context.Doctors
                    .Where(d => d.SecondaryDepartment == department)
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
                        photoUrl = doctor.Photourl, // 添加医生照片的URL
                        title = doctor.Title,
                        Count = g.Count(),
                        ConsultationInfos = g.Select(c => new { ClinicName = c.ClinicName, DateTime = c.DateTime, Period = c.Period }).ToList() // Select the clinic's name and consultation times
                    })
                    .OrderBy(r => r.DoctorId)
                    .ToList();

                result.AddRange(doctorResult);
            }

            return result;
        }

        [HttpGet("AllConsultInfo")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllConsultInfo()
        {
            var periods = await _context.ConsultationInfos.ToListAsync();
            var result = periods.Select(p => new
            {
                DoctorId = p.DoctorId,
                ClinicName = p.ClinicName,
                Date = p.DateTime.Date,
                StartTime = GetStartTime((int)p.Period),
                EndTime = GetEndTime((int)p.Period)
            });
            return Ok(result);
        }

        [HttpPut("ChangeConsult")]
        public async Task<IActionResult> ChangeConsultInfo([FromBody] ChangeConsultInputModel Change)
        {
            // 查找匹配的挂号记录
            var OldConsult = await _context.ConsultationInfos.FirstOrDefaultAsync(r =>
                r.DoctorId == Change.Old.DoctorId &&
                r.ClinicName== Change.Old.ClinicName&&
                r.DateTime == Change.Old.DateTime.Date &&
                r.Period == Change.Old.Period
                );

            // 如果找不到匹配的挂号记录，返回错误信息
            if (OldConsult == null)
            {
                return NotFound("No Changable ConsultationInfo found.");
            }

            var NewConsult = new ConsultationInfo()
            {
                DoctorId = Change.New.DoctorId,
                ClinicName= Change.New.ClinicName,
                DateTime= Change.New.DateTime,
                Period = Change.New.Period,
            };

            NewConsult.ClinicNameNavigation = await _context.ConsultingRooms.FirstOrDefaultAsync(d => d.ConsultingRoomName == Change.New.ClinicName);
            NewConsult.Doctor = await _context.Doctors.FirstOrDefaultAsync(p => p.DoctorId == Change.New.DoctorId);

            _context.ConsultationInfos.Remove(OldConsult);
            //await _context.SaveChangesAsync();
            _context.ConsultationInfos.Add(NewConsult);
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("Change ConsultationInfo Successfully.");
        }

        [HttpPut("CancelConsult")]
        public async Task<IActionResult> CancelConsultInfo([FromBody] ConsultPK canceled)
        {
            // 查找匹配的挂号记录
            var target = await _context.ConsultationInfos.FirstOrDefaultAsync(r =>
                r.DoctorId == canceled.DoctorId &&
                r.DateTime == canceled.DateTime.Date &&
                r.Period == canceled.Period
                );

            // 如果找不到匹配的挂号记录，返回错误信息
            if (target == null)
            {
                return NotFound("No Cancelabel ConsultationInfo found.");
            }

            _context.ConsultationInfos.Remove(target);
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("Cancel ConsultationInfo Successfully.");
        }

        [HttpPost("AddConsult")]
        public async Task<IActionResult> AddConsultInfo([FromBody] ConsultInputModel NewConsult)
        {
            // 查找匹配的挂号记录
            var ExistConsult =  await _context.ConsultationInfos.FirstOrDefaultAsync(r =>
                r.DoctorId == NewConsult.DoctorId &&
                r.ClinicName == NewConsult.ClinicName &&
                r.DateTime == NewConsult.DateTime.Date &&
                r.Period == NewConsult.Period
                );

            // 如果找不到匹配的挂号记录，返回错误信息
            if (ExistConsult == null)
            {
                return NotFound("Addede ConsultationInfo Already Exists.");
            }

            var TargetConsult = new ConsultationInfo()
            {
                DoctorId = NewConsult.DoctorId,
                ClinicName = NewConsult.ClinicName,
                DateTime = NewConsult.DateTime,
                Period = NewConsult.Period,
            };

            TargetConsult.ClinicNameNavigation = await _context.ConsultingRooms.FirstOrDefaultAsync(d => d.ConsultingRoomName == NewConsult.ClinicName);
            TargetConsult.Doctor = await _context.Doctors.FirstOrDefaultAsync(p => p.DoctorId == NewConsult.DoctorId);

            _context.ConsultationInfos.Add(TargetConsult);
            await _context.SaveChangesAsync();

            //// 返回成功信息
            return Ok("Add ConsultationInfo Successfully.");
        }

        private string GetStartTime(int period)
        {
            switch (period)
            {
                case 1:
                    return "08:00";
                case 2:
                    return "09:00";
                case 3:
                    return "10:00";
                case 4:
                    return "13:00";
                case 5:
                    return "14:00";
                case 6:
                    return "15:00";
                case 7:
                    return "16:00";
                default:
                    return "Unknown";
            }
        }

        private string GetEndTime(int period)
        {
            switch (period)
            {
                case 1:
                    return "09:00";
                case 2:
                    return "10:00";
                case 3:
                    return "11:00";
                case 4:
                    return "14:00";
                case 5:
                    return "15:00";
                case 6:
                    return "16:00";
                case 7:
                    return "17:00";
                default:
                    return "Unknown";
            }
        }

        public class ConsultInputModel
        {
            public string DoctorId { get; set; } = "";
            public string ClinicName { get; set; } = "";
            public DateTime DateTime { get; set; }
            public decimal Period { get; set; }
        }

        public class ConsultPK
        {
            public string DoctorId { get; set; } = null!;
            public DateTime DateTime { get; set; }
            public decimal Period { get; set; }
        }

        public class ChangeConsultInputModel
        {
            public ConsultInputModel Old;
            public ConsultInputModel New;
        }
    }
}
