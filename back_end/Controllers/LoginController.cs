using Microsoft.AspNetCore.Mvc;
using back_end.Models;
using back_end.Controllers;
using System.Web.Http.Cors;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Aop.Api.Domain;


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
        public async Task<ActionResult<bool>> CheckAdminCredentials(string ID, string password)
        {
            bool exists = await _context.Administrators.AnyAsync(
                admin => admin.AdministratorId == ID && admin.Password == password);
            if (exists)
            {
                string token = GenerateTokenAsync("Administrator");
                return Ok("The token is: " + token);
            }
            else
            {
                return BadRequest("Wrong acount or password");
            }
        }

        [HttpGet("PatientLogin")]
        public async Task<ActionResult<bool>> CheckPatientCredentials(string ID, string password)
        {
            bool exists = await _context.Patients.AnyAsync(
                patient => patient.PatientId == ID && patient.Password == password);
            if (exists)
            {
                string token = GenerateTokenAsync("Patient");
                return Ok("The token is: " + token);
            }
            else
            {
                return BadRequest("Wrong acount or password");
            }
        }

        [HttpGet("DoctorLogin")]
        public async Task<ActionResult<bool>> CheckDoctorCredentials(string ID, string password)
        {
            bool exists = await _context.Doctors.AnyAsync(
                doctor => doctor.DoctorId == ID && doctor.Password == password);
            if (exists)
            {
                string token = GenerateTokenAsync("Doctor");
                return Ok("The token is: " + token);
            }
            else
            {
                return BadRequest("Wrong acount or password");
            }
        }


        //存储验证码
        private static readonly Dictionary<string, string> VerificationCodes = new Dictionary<string, string>();
        //验证码计时器，实现“过期”功能
        private static readonly Dictionary<string, DateTime> CodeGenerateTimes = new Dictionary<string, DateTime>();
        //存储Token
        private static readonly List<string> Tokens = new List<string>();
        //Token计时器，实现“过期”功能
        private static readonly Dictionary<string, DateTime> TokenGenerateTimes = new Dictionary<string, DateTime>();
        // 定义字符集
        private static string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private string GenerateTokenAsync(string User)
        {
            //生成token
            var random = new Random();
            int length = 20;
            StringBuilder tokenBuilder = new StringBuilder();
            do
            {
                for (int i = 0; i < length; i++)
                {
                    // 随机选择一个字符并添加到StringBuilder对象中
                    char c = chars[random.Next(chars.Length)];
                    tokenBuilder.Append(c);
                }
            } while (Tokens.Contains(User + "_" + tokenBuilder.ToString()));
            //存储token
            string token = tokenBuilder.ToString();
            Tokens.Add(User + "_" + token);
            TokenGenerateTimes[User + "_" + token] = DateTime.Now;//初始化Token时间

            return token;
        }


        [HttpGet("generateVerifyCode")]
        public async Task<ActionResult<string>> GenerateVerifyCode(string PhoneNumber)
        {
            //生成验证码
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            //发送验证码到手机
            //string context = $"您的验证码是：{code}。请不要把验证码泄露给其他人。";
            string context = $"{code}";
            //Task<bool> task = MessageSender.SendSmsAsync4VC(PhoneNumber, context);
            //bool isSuccess = await task;

            // Store the code and generation time
            var currentTime = DateTime.Now;
            VerificationCodes[PhoneNumber] = code;
            CodeGenerateTimes[PhoneNumber] = currentTime;

            //Console.WriteLine("hello world!");
            return Ok(code);
        }

        [HttpGet("verifyToken")]
        public ActionResult<bool> VerifyToken(string User, string token)
        {
            token = User + "_" + token;
            //先删除所有过期的token
            var currentTime = DateTime.Now;
            foreach (var entry in TokenGenerateTimes.Where(entry => (currentTime - entry.Value).TotalDays > 3).ToList())
            {
                Tokens.Remove(entry.Key);
                TokenGenerateTimes.Remove(entry.Key);
            }
            if (Tokens.Contains(token))
            {
                TokenGenerateTimes[token] = DateTime.Now;//更新Token时间
                return Ok("Token Verify succeed.");
            }
            //return BadRequest("Token not found.");
            return BadRequest(Tokens);
        }

        [HttpGet("verifyCode")]
        public ActionResult<bool> VerifyCode(string PhoneNumber, string Code)
        {
            //先删除所有过期的验证码
            var currentTime = DateTime.Now;
            foreach (var entry in CodeGenerateTimes.Where(entry => (currentTime - entry.Value).TotalSeconds > 180).ToList())
            {
                VerificationCodes.Remove(entry.Key);
                CodeGenerateTimes.Remove(entry.Key);
            }
            //然后开始验证
            if (VerificationCodes.ContainsKey(PhoneNumber) && VerificationCodes[PhoneNumber] == Code)
            {
                return Ok("VerificationCode verify succeed.");
            }
            return BadRequest("VerificationCode not found.");
        }


        [HttpPut("resetAdminPassword")]
        public async Task<ActionResult<string>> resetAdminPassword(resetPasswordInputModel adaptInfo)
        {
            var User = await _context.Administrators.FirstOrDefaultAsync(d => d.AdministratorId == adaptInfo.ID);
            if (User == null)
            {
                return NotFound("AdministratorID not found");
            }
            User.Password = adaptInfo.NewPassword;
            await _context.SaveChangesAsync();
            return Ok("Administrator Password reset successfully!");
        }
        [HttpPut("resetDoctorPassword")]
        public async Task<ActionResult<string>> resetDoctorPassword(resetPasswordInputModel adaptInfo)
        {
            var User = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == adaptInfo.ID);
            if (User == null)
            {
                return NotFound("DoctorID not found");
            }
            User.Password = adaptInfo.NewPassword;
            await _context.SaveChangesAsync();
            return Ok("Doctor Password reset successfully!");
        }

        [HttpPut("resetPatientPassword")]
        public async Task<ActionResult<string>> resetPatientPassword(resetPasswordInputModel adaptInfo)
        {
            var User = await _context.Patients.FirstOrDefaultAsync(d => d.PatientId == adaptInfo.ID);
            if (User == null)
            {
                return NotFound("PatientID not found");
            }
            User.Password = adaptInfo.NewPassword;
            await _context.SaveChangesAsync();
            return Ok("Patient Password reset successfully!");
        }

        [HttpGet("judgeAdminPhoneID")]
        public async Task<ActionResult<string>> judgeAdminPhoneID(string PhoneNumber, string ID)
        {
            bool exist = await _context.Administrators.AnyAsync(d => d.Contact == PhoneNumber & d.AdministratorId == ID);
            if (exist) {
                return Ok("Qualified Administrator Found.");
            }
            else {
                return NotFound("No Qaulified Administrator found!");
            }
        }
        [HttpGet("judgeDoctorPhoneID")]
        public async Task<ActionResult<string>> judgeDoctorPhoneID(string PhoneNumber, string ID)
        {
            bool exist = await _context.Doctors.AnyAsync(d => d.Contact == PhoneNumber & d.DoctorId== ID);
            if (exist)
            {
                return Ok("Qualified Doctor Found.");
            }
            else
            {
                return NotFound("No Qaulified Doctor found!");
            }
        }
        [HttpGet("judgePatientPhoneID")]
        public async Task<ActionResult<string>> judgePatientPhoneID(string PhoneNumber, string ID)
        {
            bool exist = await _context.Patients.AnyAsync(d => d.Contact == PhoneNumber & d.PatientId== ID);
            if (exist)
            {
                return Ok("Qualified Patient Found.");
            }
            else
            {
                return NotFound("No Qaulified Patient found!");
            }
        }
    }

    public class resetPasswordInputModel
    {
        public string ID { get; set; } 
        public string NewPassword { get; set; }
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