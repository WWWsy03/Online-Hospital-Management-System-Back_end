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

        [HttpGet("whether")]//在评价之前先查询是否评价过了
        public async Task<IActionResult> GetTreatmentFeedback(string diagnosedId)
        {
            var record = await _context.TreatmentRecord2s
                .FirstOrDefaultAsync(r => r.DiagnoseId == diagnosedId);

            if (record == null)
            {
                return NotFound("No record found for this diagnosedId.");
            }

            if (record.Commentstate == 0)
            {
                return Ok("未评价");
            }
            else if (record.Commentstate == 1)
            {
                return Ok("已评价");
            }
            else
            {
                return BadRequest("Invalid Commentstate value.");
            }
        }


        [HttpPost]//评价信息
        public IActionResult CreateFeedback(string diagnoseId, decimal treatmentScore, string evaluation)
        {
            // 从诊断记录ID中分离出病人ID和医生ID
            var patientId = diagnoseId.Substring(8, 7);
            var doctorId = diagnoseId.Substring(15, 5);

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
