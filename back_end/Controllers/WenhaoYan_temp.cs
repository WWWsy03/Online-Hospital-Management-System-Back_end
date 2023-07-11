using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    public class WenhaoYan_temp : Controller
    {
        [HttpGet("WenhaoYan_test")]
        public IActionResult Query()
        {
            WenhaoYan_model ans = new WenhaoYan_model
            {
                Date = DateTime.Now,
                department="内科",
                status="待就诊",
                appointmentNumber="123456",
                doctor="张医生",
                appointmentTime="上午",
                waitingCount=5
            };

            return Ok(ans);
        }
    }
}
