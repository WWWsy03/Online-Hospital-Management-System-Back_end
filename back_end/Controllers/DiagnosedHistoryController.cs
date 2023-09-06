using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using Aop.Api;
using Aop.Api.Request;
using Aop.Api.Response;
using Aop.Api.Domain;
using System.Text;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosedHistoryController : ControllerBase
    {
        private readonly ModelContext _context;

        public DiagnosedHistoryController(ModelContext context)
        {
            _context = context;
        }

        [HttpGet("getPatientRecords")]
        public async Task<ActionResult<IEnumerable<TreatmentRecord2>>> GetPatientRecords(string patientId)
        {
            // 查找与病人ID匹配的所有诊断记录ID
            var diagnosisRecordIds = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId)
                .Select(r => r.DiagnosisRecordId)
                .ToListAsync();

            // 使用这些诊断记录ID在TreatmentRecord2表中查询每个ID对应的属性
            var patientRecords = await _context.TreatmentRecord2s
                .Where(r => diagnosisRecordIds.Contains(r.DiagnoseId))
                .ToListAsync();

            return Ok(patientRecords);
        }

        [HttpPost("alipayNotify")]
        public async Task<IActionResult> NotifyUrl([FromForm] Dictionary<string, string> parameters)
        {

            if (parameters.ContainsKey("out_trade_no"))
            {
                var prescriptionId = parameters["out_trade_no"];
                string diagnosedId = prescriptionId.Remove(8, 3);
                // 在数据库中获取诊断信息
                var treatment = _context.TreatmentRecords.FirstOrDefault(t => t.DiagnosisRecordId == diagnosedId);
                var prescription = _context.Prescriptions.FirstOrDefault(p => p.PrescriptionId == prescriptionId);
                prescription.Paystate = 1;  // 修改状态值，表示已经支付
                var existingOrder = await _context.OutpatientOrders.FirstOrDefaultAsync(o => o.OrderId == prescriptionId);

                if (existingOrder == null)
                {
                    var order = new OutpatientOrder
                    {
                        OrderId = prescriptionId,
                        PatientId = treatment.PatientId,
                        OrderTime = DateTime.Now
                    };

                    await _context.OutpatientOrders.AddAsync(order);
                    await _context.SaveChangesAsync();
                }
            }

            // 处理业务逻辑
            return Ok("success"); // 返回给支付网关一个确认消息
        }


        [HttpGet("alipayReturn")]
        public async Task<IActionResult> ReturnUrl([FromQuery] Dictionary<string, string> parameters)
        {

            if (parameters.ContainsKey("out_trade_no"))
            {
                var prescriptionId = parameters["out_trade_no"];
                string diagnosedId = prescriptionId.Remove(8, 3);
                // 在数据库中获取诊断信息
                var treatment = _context.TreatmentRecords.FirstOrDefault(t => t.DiagnosisRecordId == diagnosedId);
                var prescription = _context.Prescriptions.FirstOrDefault(p => p.PrescriptionId == prescriptionId);
                prescription.Paystate = 1;  // 修改状态值，表示已经支付
                var existingOrder = await _context.OutpatientOrders.FirstOrDefaultAsync(o => o.OrderId == prescriptionId);

                if (existingOrder == null)
                {
                    var order = new OutpatientOrder
                    {
                        OrderId = prescriptionId,
                        PatientId = treatment.PatientId,
                        OrderTime = DateTime.Now
                    };

                    await _context.OutpatientOrders.AddAsync(order);
                    await _context.SaveChangesAsync();
                }
                string htmlContent = @"
                <html>
                <head>
                    <title>Payment Complete</title>
                </head>
                <body>
                    <h1>已付款，请关闭</h1>
                    <p>订单号: " + prescriptionId + @"</p>
                    <p>PatientId: " + treatment.PatientId + @"</p>
                    <p>OrderTime: " + DateTime.Now.ToString() + @"</p>
                    <script>
                        // 使用JavaScript在页面加载后自动关闭窗口
                        window.onload = function() {
                            window.setTimeout(function() {
                                window.close();
                            }, 5000); // 5000毫秒（5秒）后关闭窗口，您可以根据需要更改此时间
                        };
                    </script>
                </body>
                </html>";
                return Content(htmlContent, "text/html");
            }
        }


        [HttpGet("payBill")]
        public async Task<IActionResult> PayBill(string diagnosedId)      /*这里后续要改成接收一个diagnoseId的参数*/
        {
            // 根据diagnoseId生成订单号
            string prescriptionId = diagnosedId.Insert(8, "000");
            // 在Prescription表中查找对应记录
            var prescription = _context.Prescriptions.FirstOrDefault(p => p.PrescriptionId == prescriptionId);
            if (prescription == null)
            {
                return NotFound("未找到相关处方信息");
            }
            // 判断支付状态
            if (prescription.Paystate != 0)
            {
                return BadRequest("该订单已支付");
            }
            // 获取TotalPrice
            decimal totalPrice = prescription.TotalPrice;

            IAopClient client = new DefaultAopClient("https://openapi-sandbox.dl.alipaydev.com/gateway.do", "9021000126614589", "MIIEowIBAAKCAQEAhveBahIvn61hab5cDfFfE+8ma04lShyeLcYEpb6u038h/7NQ9vv+AxRkcMdgbfSOHHWVT1QdJOalUyZ3PnIl0QvLaY+pZlIN2z51z0sOs6n5YZOJTmrC7GYYK92dZQWodG0YsmF+XsPKgq46M6VSTZhIPg0S8Q2v1Gb23Z/i4H8Ac/7WPjmEtFFDOfjJhUZsovoIlnfF4VrsrDjP7t06bO16ZwpC3bdagx5MIX6LAtHIhh2rkiEY823/OnmK2BPIVrF9YjF1fvKn3QxkYfkappD3/cM9uWh8AMyEkznLS8uwcieCbuNv3H2c+5c7k0t73fBT5SmvCXreMGabvWn4FQIDAQABAoIBAC/0hWEg8Rb1TeV6o864cqXslWQPMiSxImr1LvWNWSUAyR3Hov7+7nQ9rKp9zP+Eo3HtPY4gPvK7mQaAZmIjwNgULsRlLTWT9iRufwGWk7S2sks/VswsFvJUHEaJycD5T69+jAXlqjcVrkDckwWCukmj0BdsIczQpib8Jr78bmqBa+JKfehHHGAEYJep+MAV9PVuUWR/rkUXHoheJZbERde1nqwSXijD9sYt51WLXNEzsjuf1hlraGe7ozqjuIINeBiFyocYFM6f0svymoFG8mNO0CgkU3BxWPB6m5fvOmkbvqyrui84iadx9oBLVZVl/vTAF3XbnlUvEIi0Ld3GCEECgYEA+TVLgUYSSjY/0PrjTWJgoJZWWluEkDXHDdeYx1A+ti2Xm09ofE887ENqpVaY0/h0Sh2+q4xySXpxHnbQrIHl2gj7NdyBOFjXms9Qt1UvPYW8Uj9R75KVlq/m+xbs6Kqyi+fOZNc5BJZvApobJsWEkqBwGHdIciOx1gi7HGUXp0kCgYEAiqUob1iI+kEy1qmmpD2mCz+BG3R6u14UGv5wFFnSbBr5XbMSZJFmtoUhdlaZFbC3eQGZ13EH8xRqrDuzY0wcAmwT9NbyiDpv6C7llD/ZdOIFcYkB/MSCXMw6sZWv0zZejz2yBgLvQI1d2LDAVOlEFPFvTKr+PLwgK9fY+7Tjzm0CgYB+PrFxW74IOlM52t8rZJruvzofrB0LsTKVoJKU5eHfCFm1JBUaZEnIpp5wA96IA2Vl5oug/BUphA2qESbFPUjjm4knT/1mPht7IWsSdOTplcZBJDKt2uRM4e9xY7vAYjjxBw1XqHAKEutJtifrDESMwxoGSuc4azy74NBpIg1JgQKBgFmxIKhvsSWcWiQu2kQ0MZ/jNEWro95krUMNSTqRJSSUiq/IMeTnf3giRhSFT0GN8hORKpIKaGcj1SKY+KMLUK9sdbiV+Y6Rp2WgORsf9zC7K2RYivWXtvILmQjbWkScTq4B7pIfAeJT0dtl9Pa5dTbLPgJuOEzYM0PJvnCPhDQ9AoGBAMag8PMI/jsGf+dZ3pd56mVPMLP1PLab0m1PO5/iLvt5nezVB18JmQfFBt+J63Wv2P/nDXh/09FnlOQ108/3ikgFFSILBLq+nXhikXqVYL9GmSZTBEKzXqfw4G3fjZo1eTglMt4u7dbFnAF+10l7g+Wr3kGyViBXBOvu4ITulscI", "json", "1.0", "RSA2", "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApzj6PN6aFQTBN6MRHyt+nd7O+OjkIwXP6jmKQlgkGetdntvFvpb2q3cwe7w0uwlwoBc51gw2zUDea/8IYke8ck9UCiXH37kVCNX3bOivpykSTB2NR2xo9ALF1XkLr32xXv9Vo3/5qVkITsJN9xCpizLm+9FoBM/SKxp84Kqyg/O5ELkZ+pqlQPIqPYqEEMWyLAXNzZN02u97Y5RLCqsomW25iQphjqI1717vWc9MSEGr+qZNMrx4i7YI651h8ktrl+aRe+tC44P4tyRmpARGPkBGX9EldTGrk6PVgFYAFbmXpuengqNpzsXP3HvyLucy/e/o03sl4X7eFXW/k1RASwIDAQAB", "utf-8", false);
            AlipayTradePagePayRequest request = new AlipayTradePagePayRequest();
            //异步接收地址
            request.SetNotifyUrl("http://124.223.143.21:4999/api/DiagnosedHistory/alipayNotify");
            //同步跳转地址
            request.SetReturnUrl("http://124.223.143.21:4999/api/DiagnosedHistory/alipayReturn");


            /******必传参数******/
            Dictionary<string, object> bizContent = new Dictionary<string, object>();
            //商户订单号，商家自定义，保持唯一性
            bizContent.Add("out_trade_no", prescriptionId);       /*这里20210817010后续要改成基于diagnoseId生成的订单号*/
            //支付金额，最小值0.01元
            bizContent.Add("total_amount", totalPrice);     /*这里12后续要改成药品收款*/
            //订单标题，不可使用特殊符号
            bizContent.Add("subject", "药品收款");
            //电脑网站支付场景固定传值FAST_INSTANT_TRADE_PAY
            bizContent.Add("product_code", "FAST_INSTANT_TRADE_PAY");

            string Contentjson = JsonConvert.SerializeObject(bizContent);
            request.BizContent = Contentjson;
            AlipayTradePagePayResponse response = await Task.Run(() => client.pageExecute(request)); // 使用Task.Run转为异步

            await _context.SaveChangesAsync();
            return Content(response.Body, "text/html");
        }

        [HttpGet("GetDetailPre")]
        public async Task<IActionResult> GetDetailPre(string patientId)
        {
            // 查找与病人ID匹配的所有诊断记录ID
            var diagnosisRecordIds = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId)
                .Select(r => r.DiagnosisRecordId)
                .ToListAsync();

            // 使用这些诊断记录ID在TreatmentRecord2表中查询每个ID对应的属性
            var patientRecords = await _context.TreatmentRecord2s
                .Where(r => diagnosisRecordIds.Contains(r.DiagnoseId))
                .ToListAsync();

            // 根据病人ID查询病人的个人信息
            var patientInfo = await _context.Patients
                .Where(p => p.PatientId == patientId)
                .Select(p => new
                {
                    p.PatientId,
                    p.Name,
                    p.Gender,
                    p.BirthDate,
                    p.Contact,
                    p.Password,
                    p.College,
                    p.Counsellor
                })
                .FirstOrDefaultAsync();

            if (patientInfo == null)
            {
                return NotFound("No patient found with this patientId.");
            }

            // 根据找到的diagnoseId，将其第八位之后加“000”在拼接其后半部分得到处方ID
            var prescriptionIds = diagnosisRecordIds
                .Select(id => id.Substring(0, 8) + "000" + id.Substring(8))
                .ToList();

            // 在PrescriptionMedicine表中查询每个处方ID对应的药品名称和数量
            var medicines = await _context.PrescriptionMedicines
                .Where(m => prescriptionIds.Contains(m.PrescriptionId))
                .Select(m => new { m.PrescriptionId, m.MedicineName, m.Quantity })
                .ToListAsync();

            // 使用找到的药品名称在MedicineDescription表中查询每种药品对应的属性
            var medicineDescriptions = await _context.MedicineDescriptions
                .Where(m => medicines.Select(x => x.MedicineName).Contains(m.MedicineName))
                .Select(m => new
                {
                    m.MedicineName,
                    m.Specification,
                    m.Singledose,
                    m.Administration,
                    m.Attention,
                    m.Frequency
                })
                .ToListAsync();
            var doctorIds = patientRecords.Select(r => r.DiagnoseId.Substring(15, 5)).ToList();

            // 在医生表中查询每个医生ID对应的信息
            var doctors = await _context.Doctors
                .Where(d => doctorIds.Contains(d.DoctorId))
                .Select(d => new { d.DoctorId, d.Name, d.Title, d.SecondaryDepartment })
                .ToListAsync();
            // 将每次诊断记录及其对应的处方和药品信息分组返回
            var records = patientRecords.Select(r => new
            {
                Record = r,
                Prescription = medicines.Where(m => m.PrescriptionId == r.DiagnoseId.Substring(0, 8) + "000" + r.DiagnoseId.Substring(8)).ToList(),
                MedicineDescriptions = medicineDescriptions.Where(m => medicines.Where(x => x.PrescriptionId == r.DiagnoseId.Substring(0, 8) + "000" + r.DiagnoseId.Substring(8)).Select(x => x.MedicineName).Contains(m.MedicineName)).ToList(),
                Doctor = doctors.FirstOrDefault(d => d.DoctorId == r.DiagnoseId.Substring(15, 5))
            }).ToList();

            return Ok(new { patientInfo, records });
        }


    }
}
