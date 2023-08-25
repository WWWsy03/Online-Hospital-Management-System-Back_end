using System;
using System.Collections.Generic;
using back_end.Models;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost]
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
