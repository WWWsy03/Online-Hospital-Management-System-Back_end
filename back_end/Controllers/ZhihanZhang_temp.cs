using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    public class ZhihanZhang_temp : Controller
    {
        [HttpGet("Instructor")]
        public IActionResult Query()
        {
            List<Instructor> instructors = new List<Instructor>();

            instructors.Add(new Instructor
            {
                name = "穆斌",
                description = "副院长",
                img_link = "https://faculty.tongji.edu.cn/_resources/group1/M00/00/03/wKhyGGCWAWiAWN0vAACDLAAhQ28337.png"
            });

            instructors.Add(new Instructor
            {
                name = "张林",
                description = "教授",
                img_link = "https://sse.tongji.edu.cn/__local/4/61/23/D4CC1BF30B25024A09FD10DBA3E_34939B81_16FC6.png"
            });

            instructors.Add(new Instructor
            {
                name = "韩丰夏",
                description = "讲师",
                img_link = "https://sse.tongji.edu.cn/__local/4/AA/0B/7A732F5EB5FA03E55ED15FCB9BF_A2189F87_491C.jpg"
            });

            return Ok(instructors);
        }
    }
}
