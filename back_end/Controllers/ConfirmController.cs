using System;
using System.Collections.Generic;
using back_end.Models;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TreatmentController : ControllerBase
    {
        private readonly ModelContext _context;
        private static readonly object _lock = new object();

        public TreatmentController(ModelContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateTreatment(string doctorId, string patientId, int leaveDays)
        {
            lock (_lock)//加锁，防止有多个用户同时向其中添加记录
            {
                // 生成诊断记录ID
                var diagnoseId = DateTime.Now.ToString("yyyyMMdd") + patientId + doctorId;


                // 创建就诊记录
                var treatmentRecord = new TreatmentRecord
                {
                    DiagnosisRecordId = diagnoseId,
                    DoctorId = doctorId,
                    PatientId = patientId,
                    LeaveNoteId = leaveDays > 0 ? diagnoseId + "9" + leaveDays.ToString("D3") : null
                };

                // 创建就诊记录时间
                var treatmentRecord2 = new TreatmentRecord2
                {
                    DiagnoseId = diagnoseId,
                    DiagnoseTime = DateTime.Now
                };

                try
                {
                    _context.TreatmentRecords.Add(treatmentRecord);
                    _context.TreatmentRecord2s.Add(treatmentRecord2);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    //Console.WriteLine("An error occurred: " + ex.Message);

                    // 如果有内部异常，打印内部异常的信息
                    // if (ex.InnerException != null)
                   // {
                        // Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                        //}
                        return BadRequest(ex.Message);
                     
                }


                return Ok("Treatment record created successfully.");
            }
        }
    }
}
