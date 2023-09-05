using System.Collections.Generic;
using System.Text.Json;
using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RegistrationController : ControllerBase
    {
        private readonly ModelContext _context;

        public RegistrationController(ModelContext context)
        {
            _context = context;
        }

        [HttpGet("GetAllRegist")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllRegist()
        {
            return await _context.Registrations.OrderBy(r => r.AppointmentTime).ToListAsync();
        }

        [HttpGet("GetRegist")]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistFromDate(string doctorId, DateTime date)
        {
            var registrations = await _context.Registrations
                .Where(r => r.AppointmentTime.Date == date.Date &&
                (r.State == 1 || r.State == 0) && r.DoctorId == doctorId
                )
                .Include(r => r.Patient) //使用Include方法来包含该导航属性获取病人的姓名
                .Include(r => r.Doctor)
                .ToListAsync();

            var result = registrations.GroupBy(r => r.Period) // 按照挂号时间分组
     .Select(g => new
     {
         Period = g.Key,
         Count = g.Count() // 计算每组的人数
     })
     .OrderBy(r => r.Period)
     .ToList();


            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }

        [HttpGet("GetRegist/{date}/{period}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistFromDatePeriod(DateTime date, decimal period)
        {
            var registrations = await _context.Registrations
                .Where(r => r.AppointmentTime.Date == date.Date)
                .Where(r => r.Period == period)
                .Include(r => r.Patient) //使用Include方法来包含该导航属性获取病人的姓名
                .Include(r => r.Doctor)
                .ToListAsync();

            var result = registrations
                .GroupBy(r => r.Period)  // 根据 Period 属性进行分组
                .Select(g => new
                {
                    Period = g.Key, // 这是每个分组的 Period 值
                    Count = g.Count(),
                    Patients = g.Select(r => new { Id = r.PatientId, Name = r.Patient.Name, State = r.State }).ToList()
                })
                .ToList();

            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }

        [HttpGet("commit")]
        public IActionResult GetRegistrationsByDoctorId(string doctorId)
        {
            var currentDate = DateTime.Now.Date;
            var registrations = _context.Registrations
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId && r.AppointmentTime.Date == currentDate && r.Checkin == 1 && r.State == 0)//已报道的人按顺序显示
                .OrderBy(r => r.Period)
                .ThenBy(r => r.Registorder)
                .ToList();
            return Ok(registrations);
        }


        [HttpGet("Doctor/{ID}")]
        public IActionResult GetRegistFromDoctorId(string ID)
        {
            var registrations = _context.Registrations
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.DoctorId == ID)
                .ToList();

            return Ok(registrations);
        }

        [HttpGet("Patient/{ID}")]
        public async Task<IActionResult> GetRegistByPatientId(string ID)
        {
            var registrations = await _context.Registrations
                                        .Include(r => r.Doctor)
                                        .Include(r => r.Patient)
                                        .Where(r => r.PatientId == ID)
                                        .ToListAsync();

            if (!registrations.Any())
                return NotFound();

            var results = registrations.Select(reg =>
            {
                var queueCount = _context.Registrations
                                   .Where(r => r.AppointmentTime.Date == reg.AppointmentTime.Date &&
                                   r.Period == reg.Period &&
                                   r.DoctorId == reg.DoctorId &&
                                   r.Registorder < reg.Registorder)
                                   .Count();

                return new
                {
                    Doctor = reg.Doctor,
                    Patient = reg.Patient,
                    Date = reg.AppointmentTime.Date,
                    Period = reg.Period,
                    State = reg.State,
                    QueueCount = queueCount
                };
            }).ToList();

            return Ok(results);
        }


        [HttpGet("MedicalHistory/{patientId}")]
        public async Task<ActionResult<object>> GetMedicalHistory(string patientId)
        {
            // 首先，获取Registrations与Prescriptions的左外连接数据
            var registWithPrescripts = await (from r in _context.Registrations
                                              where r.PatientId == patientId
                                              join p in _context.Prescriptions on r.Prescriptionid equals p.PrescriptionId into grouping
                                              from g in grouping.DefaultIfEmpty()
                                              select new
                                              {
                                                  r.PatientId,
                                                  r.DoctorId,
                                                  AppointmentDate = r.AppointmentTime.Date,
                                                  r.Period,
                                                  r.State,
                                                  TotalPrice = g == null ? default(decimal) : g.TotalPrice,
                                                  PrescriptionId = g.PrescriptionId,
                                                  PayState = g.Paystate
                                              }).ToArrayAsync();

            // 然后，基于上述结果与PrescriptionMedicines进行左外连接
            var result = from r in registWithPrescripts
                         join pm in _context.PrescriptionMedicines on r.PrescriptionId equals pm.PrescriptionId into medicineGrouping
                         from mg in medicineGrouping.DefaultIfEmpty()
                         select new
                         {
                             r.PatientId,
                             r.DoctorId,
                             r.AppointmentDate,
                             r.Period,
                             r.State,
                             r.PrescriptionId,
                             r.TotalPrice,
                             r.PayState,
                             MedicineName = mg?.MedicineName,
                             MedicationInstruction = mg?.MedicationInstruction,
                             MedicinePrice = mg?.MedicinePrice ?? 0
                         };

            // 最后，返回结果
            if (!result.Any())
            {
                return NotFound();
            }

            return Ok(result);
        }


        //[HttpPut("ReorderRegistByPatientId")]
        //public IActionResult UpdateRegistOrder()
        //{
        //    // 之前的功能代码：
        //    var registrations = _context.Registrations
        //                    .OrderBy(r => r.AppointmentTime)
        //                    .ThenBy(r => r.PatientId)
        //                    .ToList();

        //    var groupedRecords = registrations.GroupBy(r => new
        //    {
        //        Date = r.AppointmentTime.Date,
        //        r.Period,
        //        r.DoctorId
        //    }).ToList();

        //    foreach (var group in groupedRecords)
        //    {
        //        int order = 1;
        //        foreach (var record in group)
        //        {
        //            record.Registorder = order;
        //            order++;
        //        }
        //    }

        //    _context.SaveChanges();

        //    return Ok("Records updated successfully!");
        //}

        //[HttpPut("JieChu_update-state")]
        //public async Task<IActionResult> UpdateAllRegistState()
        //{
        //    var regists = _context.Registrations.ToList();
        //    Console.WriteLine(regists.GetType().Name);

        //    foreach (var regist in regists)
        //    {
        //        regist.State = GenerateState(regist);
        //    }

        //    await _context.SaveChangesAsync();

        //    return Ok("States updated successfully.");
        //}

        //// 生成随机state的辅助方法
        //private int GenerateState(Registration regist)
        //{
        //    var random = new Random();
        //    int totalWeight = 4;
        //    int randomVal = random.Next(totalWeight);

        //    if (randomVal < 1)
        //        return -1;
        //    else if (randomVal < 3)
        //        return 0;
        //    else
        //        return 1;
        //}


        [HttpPost("regist")]
        public async Task<ActionResult<Registration>> CreateRegistration([FromBody] RegistrationInputModel input)
        {
            if (input.Period < 1 || input.Period > 7)
            {
                return BadRequest("invalid period");
            }

            var sameRecordNun = _context.Registrations.
                Where(r => r.DoctorId == input.DoctorId &&
                r.AppointmentTime.Date == input.Time.Date &&
                r.Period == input.Period && r.PatientId == input.PatientId && r.State == 0)
                .Count();
            if(sameRecordNun != 0) 
            {
                return BadRequest("您已经挂号，无需重复挂号");
            }
            // 获取当前最大的 Registorder 值
            var maxOrder = _context.Registrations
                .Where(r => r.DoctorId == input.DoctorId && r.AppointmentTime.Date == input.Time.Date && r.Period == input.Period)
                .Max(r => (int?)r.Registorder) ?? 0;

            var registration = new Registration()
            {
                PatientId = input.PatientId,
                DoctorId = input.DoctorId,
                AppointmentTime = input.Time,
                Period = input.Period,

                State = 0,
                Registorder = maxOrder + 1  // 设置 Registorder 为当前最大值加1
            };

            registration.Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == input.DoctorId);
            registration.Patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == input.PatientId);

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();
            if (input.Period == 1)
            {
                return Ok("8:00-9:00");
            }
            else if (input.Period == 2)
            {
                return Ok("9:00-10:00");
            }
            else if (input.Period == 3)
            {
                return Ok("10:00-11:00");
            }
            else if (input.Period == 4)
            {
                return Ok("13:00-14:00");
            }
            else if (input.Period == 5)
            {
                return Ok("14:00-15:00");
            }
            else if (input.Period == 6)
            {
                return Ok("15:00-16:00");
            }
            else
            {
                return Ok("16:00-17:00");
            }
            //   return Ok("successful.");
        }

        [HttpPut("cancel")]
        public async Task<IActionResult> CancelRegistration([FromBody] RegistrationInputModel inputModel)
        {
            // 查找匹配的挂号记录
            var registration = await _context.Registrations.FirstOrDefaultAsync(r =>
                r.PatientId == inputModel.PatientId &&
                r.DoctorId == inputModel.DoctorId &&
                r.AppointmentTime.Date == inputModel.Time.Date &&
                r.Period == inputModel.Period &&
                r.State == 0);

            // 如果找不到匹配的挂号记录，返回错误信息
            if (registration == null)
            {
                return NotFound("No registration found.");
            }

            // 查找与指定挂号记录具有相同的PatientId，DoctorId，AppointmentTime.Date，Period的所有记录
            var similarRegistrationsCount = await _context.Registrations.AsNoTracking().CountAsync(r =>
                r.PatientId == inputModel.PatientId &&
                r.DoctorId == inputModel.DoctorId &&
                r.AppointmentTime.Date == inputModel.Time.Date &&
                r.Period == inputModel.Period
            );

            // 从数据库中更改找到的挂号记录
            var newRegistration = new Registration
            {
                PatientId = registration.PatientId,
                DoctorId = registration.DoctorId,
                AppointmentTime = registration.AppointmentTime,
                Period = registration.Period,
                Prescriptionid = registration.Prescriptionid,
                Registorder = registration.Registorder,
                State = -similarRegistrationsCount

            };
            _context.Registrations.Remove(registration);//因为State是主键不能修改，不需要先删了再插入
            await _context.SaveChangesAsync();
            _context.Registrations.Add(newRegistration);
            await _context.SaveChangesAsync();
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                foreach (var entry in ex.Entries)
                {
                    if (entry.Entity is Registration)
                    {
                        // 使用数据库中的值重新加载实体
                        entry.Reload();

                        // 应用你的更改
                        ((Registration)entry.Entity).State = -1;

                        // 再次尝试保存
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // 返回成功信息
            return Ok("cancel successfully.");
        }


        [HttpPut("complete")]
        public async Task<IActionResult> CompleteRegistration([FromBody] RegistrationInputModel inputModel)
        {
            // 查找匹配的挂号记录
            var registration = await _context.Registrations.FirstOrDefaultAsync(r =>
                r.PatientId == inputModel.PatientId &&
                r.DoctorId == inputModel.DoctorId &&
                r.AppointmentTime.Date == inputModel.Time.Date &&
                r.Period == inputModel.Period);

            // 如果找不到匹配的挂号记录，返回错误信息
            if (registration == null)
            {
                return NotFound("No registration found.");
            }

            registration.State = 1;
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("complete successfully.");
        }

        [HttpPut("ChangeAppoint")]
        public async Task<IActionResult> ChangeAppointTime([FromBody] ChangeAppointmentInputModel Change)
        {
            DateTime currentTime = DateTime.Now;
            if (Change.New.Time.Date < currentTime.Date)
            {
                return NotFound("The AppointTime of the New Registration is wrong, it should be later than now");
            }

            // 查找匹配的挂号记录
            var OldRegistration = await _context.Registrations.FirstOrDefaultAsync(r =>
                r.PatientId == Change.Old.PatientId &&
                r.DoctorId == Change.Old.DoctorId &&
                r.AppointmentTime.Date == Change.Old.Time.Date &&
                r.Period == Change.Old.Period &&
                r.State == 0
                );

            // 如果找不到匹配的挂号记录，返回错误信息
            if (OldRegistration == null)
            {
                return NotFound("No Changable Registration found.");
            }

            OldRegistration.State = -1;

            var maxOrder = _context.Registrations
                .Where(r => r.DoctorId == Change.New.DoctorId && r.AppointmentTime.Date == Change.New.Time.Date && r.Period == Change.New.Period)
                .Max(r => (int?)r.Registorder) ?? 0;

            var NewRegistration = new Registration()
            {
                PatientId = Change.Old.PatientId,
                DoctorId = Change.New.DoctorId,
                AppointmentTime = Change.New.Time,
                Period = Change.New.Period,

                State = 0,
                Registorder = maxOrder + 1  // 设置 Registorder 为当前最大值加1
            };

            NewRegistration.Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == Change.New.DoctorId);
            NewRegistration.Patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == Change.Old.PatientId);

            _context.Registrations.Add(NewRegistration);
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("change appointTime successfully.");
        }

        [HttpPut("Checkin")]
        public async Task<IActionResult> UpdateCheckin([FromBody] RegistrationInputModel inputModel)
        {
            var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.PatientId == inputModel.PatientId &&
            r.DoctorId == inputModel.DoctorId && r.AppointmentTime == inputModel.Time && r.Period == inputModel.Period && r.State == 0);
            if (registration == null)
            {
                return NotFound();
            }

            if (registration.Checkin == 1)
            {
                return Ok("您已经报道，无需重复报道");

            }

            var currentTime = DateTime.Now.TimeOfDay;
            var period = 10;
            if (currentTime >= new TimeSpan(8, 0, 0) && currentTime < new TimeSpan(9, 0, 0))
                period = 1;
            else if (currentTime >= new TimeSpan(9, 0, 0) && currentTime < new TimeSpan(10, 0, 0))
                period = 2;
            else if (currentTime >= new TimeSpan(10, 0, 0) && currentTime < new TimeSpan(11, 0, 0))
                period = 3;
            else if (currentTime >= new TimeSpan(13, 0, 0) && currentTime < new TimeSpan(14, 0, 0))
                period = 4;
            else if (currentTime >= new TimeSpan(14, 0, 0) && currentTime < new TimeSpan(15, 0, 0))
                period = 5;
            else if (currentTime >= new TimeSpan(15, 0, 0) && currentTime < new TimeSpan(16, 0, 0))
                period = 6;
            else if (currentTime >= new TimeSpan(16, 0, 0) && currentTime < new TimeSpan(17, 0, 0))
                period = 7;

            //为测试方便，不限制报到时间，如要限制，取消注释该行即可
            //if (inputModel.Period < period)  
            // return Ok("已错过预约时间");

            registration.Checkin = 1;
            await _context.SaveChangesAsync();

            return Ok("报道成功");
        }

        [HttpGet("OfflineCheckin")]
        public List<object> GetRegistrationInfo(string patientId)
        {
            var today = DateTime.Today;
            var registrations = _context.Registrations
                .Where(r => r.PatientId == patientId && r.AppointmentTime.Date == today && r.State == 0)//找到该病人今天未就诊的挂号信息
                .ToList();

            if (registrations.Count == 0)
            {
                return new List<object>();
            }

            var result = new List<object>();
            foreach (var registration in registrations)
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.DoctorId == registration.DoctorId);
                var patient = _context.Patients.FirstOrDefault(p => p.PatientId == patientId);
                if (doctor == null)
                {
                    return new List<object>();
                }

                int registrationFee = 0;
                switch (doctor.Title)
                {
                    case "主任医师":
                        registrationFee = 9;
                        break;
                    case "副主任医师":
                        registrationFee = 7;
                        break;
                    case "主治医师":
                        registrationFee = 6;
                        break;
                    case "住院医师":
                        registrationFee = 4;
                        break;
                    case "医师":
                        registrationFee = 4;
                        break;
                    default:
                        registrationFee = 6;
                        break;
                }

                result.Add(new
                {
                    PatientName = patient.Name,
                    DoctorId=doctor.DoctorId,
                    DoctorName = doctor.Name,
                    DoctorDepartment = doctor.SecondaryDepartment,
                    AppointmentDate = registration.AppointmentTime.Date,
                    Period = registration.Period,
                    RegistrationFee = registrationFee,
                    Checkin = registration.Checkin
                });
            }
            return result;
        }

    }

    public class RegistrationInputModel//用于传输数据
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public DateTime Time { get; set; }
        public int Period { get; set; }
    }

    public class NewRegistInputModel
    {
        public string DoctorId { get; set; }
        public DateTime Time { get; set; }
        public int Period { get; set; }
    }

    public class ChangeAppointmentInputModel
    {
        public RegistrationInputModel Old { get; set; }
        public NewRegistInputModel New { get; set; }
    }

}
