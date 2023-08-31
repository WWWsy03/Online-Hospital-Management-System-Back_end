using System;
using System.Collections.Generic;
using back_end.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ModelContext _context;

        public CommentController(ModelContext context)
        {
            _context = context;
        }

        [HttpGet("whether")]//找到该病人已经评价过的诊断记录ID
        public async Task<IActionResult> GetTreatmentFeedback(string patientId)
        {
            var records = await _context.TreatmentRecord2s
                .Where(r => r.DiagnoseId.Substring(8, 7) == patientId && r.Commentstate == 1)
                .Select(r => r.DiagnoseId)
                .ToListAsync();

            if (records.Count == 0)
            {
                return Ok("No records found for this patientId.");
            }

            return Ok(records);
        }


        [HttpPost]//评价信息
        public async Task<IActionResult> CreateFeedback(string diagnoseId, decimal treatmentScore, string evaluation)
        {
            // 从诊断记录ID中分离出病人ID和医生ID
            var patientId = diagnoseId.Substring(8, 7);
            var doctorId = diagnoseId.Substring(15, 5);

            var record = await _context.TreatmentRecord2s
     .FirstOrDefaultAsync(r => r.DiagnoseId == diagnoseId && r.Commentstate == 0);

            if (record == null)
            {
                return BadRequest("无法进行评价（无此条诊断记录或已经评价过）");
            }
            record.Commentstate = 1;
            await _context.SaveChangesAsync();
            // 创建就诊反馈记录
            var treatmentFeedback = new TreatmentFeedback
            {
                PatientId = patientId,
                DoctorId = doctorId,
                TreatmentScore = treatmentScore,
                Evaluation = evaluation
            };

            try
            {
                _context.TreatmentFeedbacks.Add(treatmentFeedback);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Treatment feedback created successfully.");
        }
    }
}
