using back_end.Models;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ModelContext _context;
        //private static readonly object _lock = new object();

        public LeaveController(ModelContext context)
        {
            _context = context;
        }

        [HttpPost]//线上申请假条
        public async Task<ActionResult<LeaveApplication>> OnlineApplication(string diagnosedId,int leaveDays )
        {
            if (leaveDays == 0)
            {
                return Ok("No need for leave");
            }
            // 生成假条ID
            var leaveNoteId = diagnosedId + leaveDays.ToString("D3");

            // 获取请假开始日期
            var leaveStartDate = DateTime.ParseExact(diagnosedId.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);

            // 创建假条申请记录
            var leaveApplication = new LeaveApplication
            {
                LeaveNoteId = leaveNoteId,
                LeaveApplicationTime = DateTime.Now,
                LeaveStartDate = leaveStartDate,
                LeaveEndDate = leaveStartDate.AddDays(leaveDays),
                LeaveNoteRemark = "未审核"
            };

            try
            {
                _context.LeaveApplications.Add(leaveApplication);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Leave application created successfully.");
        }

        [HttpPost("Offline")]
        public async Task<ActionResult<LeaveApplication>> OfflineApplication(string patientId, string doctorId, string period, int leaveDays)
        {
            if (leaveDays == 0)
            {
                return Ok("No need for leave");
            }

            // 生成假条ID
            var today = DateTime.Today.ToString("yyyyMMdd");
            var diagnosedId = today + patientId + doctorId + period;
            var leaveNoteId = today + patientId + doctorId + period + leaveDays.ToString("D3");
            var diagnosis =  _context.TreatmentRecords.FirstOrDefault(t => t.DiagnosisRecordId == diagnosedId);
            if(diagnosis == null)
            {
                return Ok("未找到相关就诊记录，假条开具失败");
            }
            // 获取请假开始日期
            var leaveStartDate = DateTime.Today;

            // 创建假条申请记录
            var leaveApplication = new LeaveApplication
            {
                LeaveNoteId = leaveNoteId,
                LeaveApplicationTime = DateTime.Now,
                LeaveStartDate = leaveStartDate,
                LeaveEndDate = leaveStartDate.AddDays(leaveDays),
                LeaveNoteRemark = "通过"
            };
            var treatmentRecord = _context.TreatmentRecords.FirstOrDefault(t => t.DiagnosisRecordId == diagnosedId);
            if(treatmentRecord == null)
            {
                return BadRequest("未找到相关就诊记录");
            }
           
            try
            {
                
                _context.LeaveApplications.Add(leaveApplication);
                await _context.SaveChangesAsync();
                treatmentRecord.LeaveNoteId = leaveNoteId;
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Leave application created successfully.");
        }


        [HttpPut("ratify")]
        public async Task<IActionResult> RatifyLeaveApplication(string leaveNoteId, int status)
        {
            var leaveApplication = _context.LeaveApplications.FirstOrDefault(l => l.LeaveNoteId == leaveNoteId&&l.LeaveNoteRemark=="未审核");
            var diagnosedId = leaveNoteId.Substring(0, leaveNoteId.Length - 3);
            var treatmentRcord=_context.TreatmentRecords.FirstOrDefault(t=>t.DiagnosisRecordId==diagnosedId);
            if (leaveApplication == null)
            {
                return NotFound("未找到该假条记录（或该假条已被审核）");
            }

            if (status == 1)
            {
                leaveApplication.LeaveNoteRemark = "通过";
                treatmentRcord.LeaveNoteId = leaveNoteId;
            }
            else if (status == 0)
            {
                leaveApplication.LeaveNoteRemark = "不通过";
            }
            else
            {
                return BadRequest("Invalid status value");
            }

            await _context.SaveChangesAsync();

            return Ok("Certificate of Approval Successful");
        }
    

        [HttpGet("leaveApplications")]//传入病人Id找到其申请过的假条
        public async Task<IActionResult> GetLeaveApplicationsByStudent(string patientId)
        {
            var leaveApplications = await _context.LeaveApplications
                .Where(l => l.LeaveNoteId.Substring(8, 7) == patientId)
                .Select(l => l.LeaveNoteId)
                .ToListAsync();

            if (leaveApplications.Count == 0)
            {
                return Ok("该病人未申请过假条");
            }

            return Ok(leaveApplications);
        }

        [HttpGet("diagnosis")]
        public async Task<IActionResult> GetDiagnosedId(string patientId)
        {
            var leaveApplications = await _context.LeaveApplications
                .Where(l => l.LeaveNoteId.Substring(8, 7) == patientId)
                .Select(l => l.LeaveNoteId.Substring(0, l.LeaveNoteId.Length - 1))
                .ToListAsync();

            if (leaveApplications.Count == 0)
            {
                return Ok("该病人未申请过假条");
            }

            return Ok(leaveApplications);
        }


        [HttpGet("GetLeaveApplicationsByDoctor")]//输入医生ID返回关于该医生申请的所有假条
        public IEnumerable<object> GetLeaveApplicationsByDocotr(string doctorId)
        {
            var leaveApplications = _context.LeaveApplications
                .Where(la => la.LeaveNoteId.Substring(15, 5) == doctorId && la.LeaveNoteRemark == "未审核")
                .ToList();

            var results = new List<object>();

            foreach (var leaveApplication in leaveApplications)
            {
                var diagnoseId = leaveApplication.LeaveNoteId.Substring(0, leaveApplication.LeaveNoteId.Length - 3);
                var treatmentRecord = _context.TreatmentRecord2s
                    .Where(tr => tr.DiagnoseId == diagnoseId)
                    .Select(tr => new TreatmentRecord2
                    {
                        DiagnoseTime = tr.DiagnoseTime,
                        Selfreported = tr.Selfreported,
                        Presenthis = tr.Presenthis,
                        Anamnesis = tr.Anamnesis,
                        Sign = tr.Sign,
                        Clinicdia = tr.Clinicdia,
                        Advice = tr.Advice,
                        Kindquantity = tr.Kindquantity
                    })
                    .FirstOrDefault();

                results.Add(new
                {
                    LeaveApplication = leaveApplication,
                    TreatmentRecord = treatmentRecord
                });
            }

            return results;
        }


    }
}
