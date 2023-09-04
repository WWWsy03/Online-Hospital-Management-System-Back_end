using Microsoft.AspNetCore.Mvc;
using back_end.Models;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ModelContext _context;

        public LoginController(ModelContext context)
        {
            _context = context;
        }
        [HttpGet]
        public string login(string user_id)
        {
            Message message = new();
            try
            {
                var patient_info = _context.Patients
                    .Where(patient => patient.PatientId == user_id)
                    .Select(patient => new
                    {
                        patient.PatientId,
                        patient.Name,
                    }).First();
                message.Add("userid", patient_info.PatientId);
                message.Add("username", patient_info.Name);
                message.Add("errorCode", 200);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }
            return message.ReturnJson();
        }

        [HttpGet("AdminLogin")]
        public async Task<ActionResult<bool>> CheckAdminCredentials(string ID,string password)
        {
            bool exists = await _context.Administrators.AnyAsync(
                admin => admin.AdministratorId == ID && admin.Password == password);

            return Ok(exists);
        }

        [HttpGet("PatientLogin")]
        public async Task<ActionResult<bool>> CheckPatientCredentials(string ID, string password)
        {
            bool exists = await _context.Patients.AnyAsync(
                patient => patient.PatientId == ID && patient.Password == password);

            return Ok(exists);
        }

        [HttpGet("DoctorLogin")]
        public async Task<ActionResult<bool>> CheckDoctorCredentials(string ID, string password)
        {
            bool exists = await _context.Doctors.AnyAsync(
                doctor => doctor.DoctorId== ID && doctor.Password == password);

            return Ok(exists);
        }


        //存储验证码
        private static readonly Dictionary<string, string> VerificationCodes = new Dictionary<string, string>();
        //验证码计时器，实现“过期”功能
        private static readonly Dictionary<string, DateTime> CodeGenerateTimes = new Dictionary<string, DateTime>();
        //存储Token
        private static readonly Dictionary<string, string> Token = new Dictionary<string, string>();
        //Token计时器，实现“过期”功能
        private static readonly Dictionary<string, DateTime> TokenGenerateTimes = new Dictionary<string, DateTime>();

        [HttpGet("generate")]
        public ActionResult<string> GenerateCode(string PhoneNumber)
        {
            var currentTime = DateTime.Now;

            // Remove expired codes
            foreach (var entry in CodeGenerateTimes.Where(entry => (currentTime - entry.Value).TotalSeconds > 60).ToList())
            {
                VerificationCodes.Remove(entry.Key);
                CodeGenerateTimes.Remove(entry.Key);
            }

            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            // Store the code and generation time
            VerificationCodes[PhoneNumber] = code;
            CodeGenerateTimes[PhoneNumber] = currentTime;

            return Ok(code);
        }


        [HttpGet("verify")]
        public ActionResult<bool> VerifyCode(string PhoneNumber,string Code)
        {
            if (VerificationCodes.TryGetValue(PhoneNumber, out var storedCode))
            {
                var generatedTime= CodeGenerateTimes.GetValueOrDefault(PhoneNumber);
                if ((DateTime.Now - generatedTime).TotalSeconds <= 60)
                {
                    return Ok(storedCode == Code);
                }
                else
                {
                    // Remove expired code
                    VerificationCodes.Remove(PhoneNumber);
                    CodeGenerateTimes.Remove(PhoneNumber);
                    return BadRequest("Code expired.");
                }
            }
            return BadRequest("Phone number not found.");
        }

    }

}



/*
namespace test.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ModelContext myContext; // 创建一个模型上下文对象

        // 构造函数，在每次调用该Controller中的api函数时，都会创建一个context对象用于操作数据库
        public LoginController(ModelContext modelContext)
        {
            myContext = modelContext;
        }

        // 在此处敲api函数
        [HttpPost]
        public string login(string user_phone, string password)
        {
            Message message = new();
            try
            {
                var user_info = myContext.Users
                .Where(user => user.UserPhone == user_phone && user.UserPassword == password)
                .Select(user => new
                {
                    user.UserId,
                    user.UserName,
                }).First();
                message.Add("userid", user_info.UserId);
                message.Add("username", user_info.UserName);
                message.Add("errorCode", 200);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.ToString());
            }

            return message.ReturnJson();
        }
    }////
}
*/