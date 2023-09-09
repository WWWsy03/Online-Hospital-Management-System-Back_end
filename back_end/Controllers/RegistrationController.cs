using System.Collections.Generic;
using System.Text.Json;
using back_end.Models;
using back_end.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> GetRegistFromDoctorId(string ID)
        {
            var registrations = await _context.Registrations
                                        .Include(r => r.Doctor)
                                        .Include(r => r.Patient)
                                        .Where(r => r.DoctorId == ID)
                                        .ToListAsync();

            if (!registrations.Any())
                return NotFound("No Doctor Found!");

            var results = registrations.Select(reg =>
            {
                var queueCount = _context.Registrations
                                   .Where(r => r.AppointmentTime.Date == reg.AppointmentTime.Date &&
                                   r.Period == reg.Period &&
                                   r.DoctorId == reg.DoctorId &&
                                   r.Registorder < reg.Registorder)
                                   .Count();
                var payState = _context.Prescriptions.FirstOrDefault(p => p.PrescriptionId == reg.Prescriptionid)?.Paystate;
                return new
                {
                    Doctor = reg.Doctor,
                    Patient = reg.Patient,
                    Date = reg.AppointmentTime.Date,
                    Period = reg.Period,
                    State = reg.State,
                    PayState = payState,
                    QueueCount = queueCount
                };
            }).ToList();

            return Ok(results);
        }

        [HttpGet("GetPatientInfoByDoctorID/{ID}")]
        public async Task<IActionResult> GetPatientInfoByDoctorID(string ID)
        {
            var registrations = await _context.Registrations
                                        .Include(r => r.Doctor)
                                        .Include(r => r.Patient)
                                        .Where(r => r.DoctorId == ID && r.State==1)
                                        .ToListAsync();

            if (!registrations.Any())
                return NotFound("No Doctor Found!");

            var results = registrations.Select(reg =>
            {
                return new
                {
                    PatientID=reg.Patient.PatientId,
                    PatientName = reg.Patient.Name,
                    PatientGender=reg.Patient.Gender,
                    AppointmentDate= reg.AppointmentTime.Date,
                    Period=reg.Period
                };
            }).ToList();

            return Ok(results);
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
                return NotFound("No Patient Found");

            var results = registrations.Select(reg =>
            {
                var queueCount = _context.Registrations
                                   .Where(r => r.AppointmentTime.Date == reg.AppointmentTime.Date &&
                                   r.Period == reg.Period &&
                                   r.DoctorId == reg.DoctorId &&
                                   r.Registorder < reg.Registorder&&
                                   r.State==0)
                                   .Count();
                if (reg.State == 1 || reg.State == -1)
                    queueCount = 0;
                var payState = _context.Prescriptions.FirstOrDefault(p => p.PrescriptionId == reg.Prescriptionid)?.Paystate;
                return new
                {
                    Doctor = reg.Doctor,
                    Patient = reg.Patient,
                    OrderTime = reg.Ordertime,
                    Date = reg.AppointmentTime.Date,
                    Period = reg.Period,
                    State = reg.State,
                    PayState = payState,
                    QueueCount = queueCount
                };
            }).ToList();

            return Ok(results);
        }


        [HttpPost("regist")]
        public async Task<ActionResult<Registration>> CreateRegistration([FromBody] RegistrationInputModel input)
        {
            if (input.Period < 1 || input.Period > 7)
            {
                return BadRequest("invalid period");
            }

            var sameRecordNum = _context.Registrations.
                Where(r => r.DoctorId == input.DoctorId &&
                r.AppointmentTime.Date == input.Time.Date &&
                r.Period == input.Period && r.PatientId == input.PatientId && r.State == 0)
                .Count();
            if(sameRecordNum != 0) 
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
                Qrcodeurl=input.QRCodeUrl,
                Ordertime = DateTime.Now,
                State = 0,
                Registorder = maxOrder + 1  // 设置 Registorder 为当前最大值加1
            };

            registration.Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == input.DoctorId);
            registration.Patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == input.PatientId);

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            string DoctorName = registration.Doctor.Name;
            string Date= registration.AppointmentTime.Date.ToString("yyyy-MM-dd");
            string Clock = period2clock(input.Period);
            string Qrcodeurl = input.QRCodeUrl;
            string PhoneNumber = registration.Patient.Contact;
            string messageContext = $"您预约的{DoctorName}医生 {Date} {Clock} 已预约成功，" +
                $"您的预约二维码地址为：{Qrcodeurl}，" +
                $"请在预约时间前携带二维码于报到处进行报到";
            //MessageSender.SendSmsAsync(PhoneNumber,messageContext);

            return Ok(Clock);
        }

        [HttpPut("cancel")]
        public async Task<IActionResult> CancelRegistration([FromBody] RegistrationInputModel2 inputModel)
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
                Qrcodeurl = registration.Qrcodeurl,
                Ordertime = registration.Ordertime,
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
                Ordertime = DateTime.Now,
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
        public async Task<IActionResult> UpdateCheckin([FromBody] RegistrationInputModel2 inputModel)
        {
            var registration = await _context.Registrations.FirstOrDefaultAsync(r => r.PatientId == inputModel.PatientId &&
            r.DoctorId == inputModel.DoctorId && r.AppointmentTime == inputModel.Time && r.Period == inputModel.Period && r.State == 0);
            if (registration == null)
            {
                return NotFound();
            }

            if (registration.Checkin == 1)
            {
                return Ok("您已经报到，无需重复报到");

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

            return Ok("报到成功");
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

        private string period2clock(int period)
        {
            string clock = "";
            switch (period)
            {
                case 1:
                    clock = "8:00-9:00";
                    break;
                case 2:
                    clock = "9:00-10:00";
                    break;
                case 3:
                    clock = "10:00-11:00";
                    break;
                case 4:
                    clock = "13:00-14:00";
                    break;
                case 5:
                    clock = "14:00-15:00";
                    break;
                case 6:
                    clock = "15:00-16:00";
                    break;
                case 7:
                    clock = "16:00-17:00";
                    break;
            }
            return clock;
        }
    }

    public class RegistrationInputModel//用于传输数据
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public DateTime Time { get; set; }
        public int Period { get; set; }
        public string QRCodeUrl { get; set; }
    }

    public class RegistrationInputModel2//用于传输数据
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
