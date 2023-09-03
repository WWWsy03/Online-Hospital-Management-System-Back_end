using back_end.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollController : ControllerBase
    {
        private readonly ModelContext _context;
        public EnrollController(ModelContext context)
        {
            _context = context;
        }

        [HttpPost("AdminEnroll")]
        public async Task<IActionResult> AdminEnroll(AdminEnrollInputModel inputModel)
        {
            // 查找数据库中是否已有同样的人存在
            bool exists = await _context.Administrators.AnyAsync(r =>
                r.AdministratorId == inputModel.administratorId 
                //&&
                //r.Name == inputModel.Name &&
                //r.Gender == inputModel.Gender &&
                //r.Birthdate.Value.Date == inputModel.Birthdate.Date&&
                //r.Contact==inputModel.Contact
                );

            // 若存在同样的人
            if (exists)
            {
                return NotFound("This person has been in Database.");
            }

            var admin = new Administrator()
            {
                AdministratorId = inputModel.administratorId,
                Name = inputModel.name,
                Gender = inputModel.gender,
                Birthdate = inputModel.birthdate,
                Contact=inputModel.contact,
                Password = inputModel.password
            };

            //新的管理员还没有购买过药品
            //admin.MedicinePurchases = await _context.MedicinePurchases.Where(d => d.AdministratorId == inputModel.AdministratorId).ToArrayAsync();
            //admin.MedicineStocks = await _context.MedicineStocks.Where(d => d.AdministratorId == inputModel.AdministratorId).ToArrayAsync();

            _context.Administrators.Add(admin);
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("Enroll successfully.");
        }

        [HttpPost("DoctorEnroll")]
        public async Task<IActionResult> DoctorEnroll(DoctorEnrollInputModel inputModel)
        {
            // 查找数据库中是否已有同样的人存在
            bool exists = await _context.Doctors.AnyAsync(r =>
                r.DoctorId == inputModel.doctorId 
                //&&
                //r.Name == inputModel.Name &&
                //r.Gender == inputModel.Gender &&
                //r.Birthdate.Value.Date == inputModel.Birthdate.Date &&
                //r.Title==inputModel.Title&&
                //r.Contact == inputModel.Contact&&
                //r.SecondaryDepartment==inputModel.SecondaryDepartment&&
                //r.Photourl==inputModel.Photourl
                );

            // 若存在同样的人
            if (exists)
            {
                return NotFound("This person has been in Database.");
            }

            var doctor = new Doctor()
            {
                DoctorId = inputModel.doctorId ,
                Name = inputModel.name ,
                Gender = inputModel.gender ,
                Birthdate = inputModel.birthdate ,
                Title=inputModel.title,
                Contact = inputModel.contact,
                SecondaryDepartment=inputModel.secondaryDepartment,
                //Photourl=inputModel.Photourl,//毛灵俊说在个人信息页面上传照片
                Password=inputModel.password
            };

            //新的管理员还没有购买过药品
            //admin.MedicinePurchases = await _context.MedicinePurchases.Where(d => d.AdministratorId == inputModel.AdministratorId).ToArrayAsync();
            //admin.MedicineStocks = await _context.MedicineStocks.Where(d => d.AdministratorId == inputModel.AdministratorId).ToArrayAsync();

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("Enroll successfully.");
        }

        [HttpPost("PatientEnroll")]
        public async Task<IActionResult> PatientEnroll(PatientEnrollInputModel inputModel)
        {
            // 查找数据库中是否已有同样的人存在
            bool exists = await _context.Patients.AnyAsync(r =>
                r.PatientId == inputModel.patientId 
                //&&
                //r.Name == inputModel.Name &&
                //r.Gender == inputModel.Gender &&
                //r.BirthDate.Value.Date == inputModel.BirthDate.Date &&
                //r.Contact == inputModel.Contact &&
                //r.College == inputModel.College&&
                //r.Counsellor == inputModel.Counsellor
                );

            // 若存在同样的人
            if (exists)
            {
                return NotFound("This person has been in Database.");
            }

            var patient = new Patient()
            {
                PatientId = inputModel.patientId ,
                Name = inputModel.name ,
                Gender = inputModel.gender ,
                BirthDate= inputModel.birthDate ,
                Contact = inputModel.contact ,
                College = inputModel.College,
                //Counsellor = inputModel.Counsellor,//毛灵俊是counsellor他统一在个人信息页面上传
                Password= inputModel.password
            };

            //新的管理员还没有购买过药品
            //admin.MedicinePurchases = await _context.MedicinePurchases.Where(d => d.AdministratorId == inputModel.AdministratorId).ToArrayAsync();
            //admin.MedicineStocks = await _context.MedicineStocks.Where(d => d.AdministratorId == inputModel.AdministratorId).ToArrayAsync();

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // 返回成功信息
            return Ok("Enroll successfully.");
        }


        public class AdminEnrollInputModel//用于传输数据
        {
            public string administratorId { get; set; } = null!;
            public string name { get; set; }
            public bool gender { get; set; }
            public DateTime birthdate { get; set; }
            public string contact { get; set; }
            public string password { get; set; }
        }

        public class DoctorEnrollInputModel//用于传输数据
        {
            public string doctorId { get; set; } = null!;
            public string name { get; set; }
            public bool gender { get; set; }
            public DateTime birthdate { get; set; }
            public string title { get; set; }
            public string contact { get; set; }
            public string secondaryDepartment { get; set; }
            //public string Photourl { get; set; }
            public string password { get; set; }
        }

        public class PatientEnrollInputModel//用于传输数据
        {
            public string patientId { get; set; } = null!;
            public string name { get; set; }
            public bool gender { get; set; }
            public DateTime birthDate { get; set; }
            public string contact { get; set; }
            public string password { get; set; } = null!;
            public string College { get; set; } = null!;
            //public string Counsellor { get; set; } = null!;
        }
    }
}
