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

        [HttpGet("GetAllFeedbacks")]//获取所有反馈信息
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var result = await _context.TreatmentFeedbacks.Select(t=>new { t.Diagnosedid, t.PatientId, t.DoctorId,t.TreatmentScore ,t.Evaluation})
                .ToListAsync();
            return Ok(result);
        }


        [HttpDelete("DeleteFeedback")]
        public async Task<ActionResult> DeleteFeedback(string diagnosedId)
        {
            var record = await _context.TreatmentFeedbacks.FirstOrDefaultAsync(r => r.Diagnosedid == diagnosedId);
            if (record == null)
            {
                return NotFound("未找到相关评价记录");
            }
            var treatmentRcord = await _context.TreatmentRecord2s.FirstOrDefaultAsync(t => t.DiagnoseId == diagnosedId);
            if (treatmentRcord == null)
            {
                return BadRequest("无对应诊断记录");

            }
            treatmentRcord.Commentstate = 0;//删除反馈之后对应诊断记录改为未评价
            _context.TreatmentFeedbacks.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
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
                Evaluation = evaluation,
                Diagnosedid = diagnoseId
            };

            try
            {
                _context.TreatmentFeedbacks.Add(treatmentFeedback);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Treatment feedback created successfully.");
        }
    }
}
