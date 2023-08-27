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
        //private static readonly object _lock = new object();

        public ConfirmController(ModelContext context)
        {
            _context = context;
        }

       
        [HttpPost]
        public async Task<ActionResult<TreatmentRecord2>> PostTreatmentRecord2([FromBody]TreatmentRecordModel inputModel)
        {
            // 生成诊断记录ID
            var diagnoseId = DateTime.Now.ToString("yyyyMMdd") + inputModel.patientId + inputModel.doctorId;

            //生成处方订单号
            var prescriptionId = DateTime.Now.ToString("yyyyMMdd") + "000"+inputModel.patientId + inputModel.doctorId;
            var totalprice = 0.0M; //总药费

            // 创建就诊记录
            var treatmentRecord = new Models.TreatmentRecord
            {
                DiagnosisRecordId = diagnoseId,
                DoctorId = inputModel.doctorId,
                PatientId = inputModel.patientId,

            };

            var treatmentRecord2 = new TreatmentRecord2
            {
                DiagnoseId = diagnoseId, // 假设DiagnoseId是病人id
                DiagnoseTime = DateTime.Now,
                Commentstate = 0,
                Selfreported = inputModel.selfReported,
                Presenthis = inputModel.presentHis,
                Anamnesis = inputModel.anamnesis,
                Sign = inputModel.sign,
                Clinicdia = inputModel.clinicDia,
                Advice = inputModel.advice
            };

            try
            {
                // 查找匹配的挂号记录
                var registration = _context.Registrations.FirstOrDefault(r =>
                    r.PatientId == inputModel.patientId &&
                    r.DoctorId == inputModel.doctorId &&
                    r.AppointmentTime.Date == DateTime.Now.Date &&
                    r.State == 0
                    );

                if (registration != null)
                {
                    registration.State = 1;//挂号表中改成已就诊
                    registration.Prescriptionid = prescriptionId;//加入处方编号
                }
                _context.TreatmentRecords.Add(treatmentRecord);
                _context.TreatmentRecord2s.Add(treatmentRecord2);


                // 解析药品信息
                var medicines = inputModel.medicine.Split(';');
                foreach (var medicine in medicines)
                {
                    var medicineInfo = medicine.Split('+');

                    if (medicineInfo.Length != 2)
                    {
                        continue;
                    }

                    var medicineName = medicineInfo[0];
                    var medicationInstruction = medicineInfo[1];

                    // 从MedicineSell表中获取药品价格
                    var medicineSell = _context.MedicineSells.FirstOrDefault(m => m.MedicineName == medicineName);

                    if (medicineSell == null)
                    {
                        continue;
                    }

                    var prescriptionMedicine = new PrescriptionMedicine
                    {
                        PrescriptionId = prescriptionId,
                        MedicineName = medicineName,
                        MedicationInstruction = medicationInstruction,
                        MedicinePrice = medicineSell.SellingPrice
                    };
                    totalprice += medicineSell.SellingPrice;
                    _context.PrescriptionMedicines.Add(prescriptionMedicine);
                }
                // 创建新的Prescription对象
                var prescription = new Prescription
                {
                    PrescriptionId = prescriptionId,
                    TotalPrice = totalprice, // 这里假设totalprice是计算出的总价
                    DoctorId = inputModel.doctorId,
                    Paystate = 0 // 初始值为0
                };

                // 将新的Prescription对象添加到数据库上下文中
                _context.Prescriptions.Add(prescription);

                await _context.SaveChangesAsync();
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

    public class TreatmentRecordModel
    {
        public string patientId { get; set; }
        public string doctorId { get; set; }
        public string selfReported { get; set; }
        public string presentHis { get; set; }
        public string anamnesis { get; set; }
        public string sign { get; set; }
        public string clinicDia { get; set; }
        public string advice { get; set; }
        public string medicine { get; set; }
    }


}
