﻿using System.Collections.Generic;
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
            return await _context.Registrations.ToListAsync();
        }

        [HttpGet("GetRegist")]
        public async Task<ActionResult<IEnumerable<object>>> GetRegistFromDate(DateTime date)
        {
            var registrations = await _context.Registrations
                .Where(r => r.AppointmentTime.Date == date.Date&&(r.State==1||r.State==0))
                .Include(r => r.Patient) //使用Include方法来包含该导航属性获取病人的姓名
                .Include(r => r.Doctor)
                .ToListAsync();

            var result = registrations.GroupBy(r => r.Period)//按照挂号时间分组
                .Select(g => new
                {
                    Period = g.Key,
                    Count = g.Count(),
                    Patients = g.Select(r => new { Id = r.PatientId, Name = r.Patient.Name }).ToList() // Select the patient's ID and name
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
                    Patients = g.Select(r => new { Id = r.PatientId, Name = r.Patient.Name ,State=r.State}).ToList()
                })
                .ToList();

            var json = JsonSerializer.Serialize(result);

            return Content(json, "application/json");
        }

        [HttpGet("commit")]
        public IActionResult GetRegistrationsByDoctorId(string doctorId)//传入医生ID获取当天该医生名下的挂号人
        {
            var currentDate = DateTime.Now.Date;
            var registrations = _context.Registrations
                .Include(r => r.Patient)
                .Where(r => r.DoctorId == doctorId && r.AppointmentTime.Date == currentDate)
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
                                   r.DoctorId == reg.DoctorId&&
                                   r.Registorder < reg.Registorder)
                                   .Count();

                return new
                {
                    Doctor = reg.Doctor,
                    Patient = reg.Patient,
                    Date = reg.AppointmentTime.Date,
                    Period = reg.Period,
                    State=reg.State,
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

            return Ok("successful.");
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
                Registorder=registration.Registorder,
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
