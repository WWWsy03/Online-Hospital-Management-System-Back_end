using System;
using System.Collections.Generic;
using back_end.Models;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfirmController : ControllerBase
    {
        private readonly ModelContext _context;
        private static readonly object _lock = new object();

        public ConfirmController(ModelContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult CreateTreatment(string doctorId, string patientId)
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
              
                };

                // 创建就诊记录时间
                var treatmentRecord2 = new TreatmentRecord2
                {
                    DiagnoseId = diagnoseId,
                    DiagnoseTime = DateTime.Now
                };

                try
                {
                    // 查找匹配的挂号记录
                    var registration = _context.Registrations.FirstOrDefault(r =>
                        r.PatientId == patientId &&
                        r.DoctorId == doctorId &&
                        r.AppointmentTime.Date == DateTime.Now.Date &&
                        r.State == 0
                        ) ;

                    if (registration != null)
                    {
                        registration.State = 1;
                        _context.SaveChanges();
                    }
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
