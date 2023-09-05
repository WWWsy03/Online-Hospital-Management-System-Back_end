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
        public async Task<ActionResult<TreatmentRecord2>> PostTreatmentRecord2([FromBody] TreatmentRecordModel inputModel)
        {
            // 生成诊断记录ID
            var diagnoseId = DateTime.Now.ToString("yyyyMMdd") + inputModel.patientId + inputModel.doctorId + inputModel.period;
            var prescriptionId = DateTime.Now.ToString("yyyyMMdd") + "000" + inputModel.patientId + inputModel.doctorId + inputModel.period;
            var totalprice = 0.0M; //总药费
            var treatmentRecord = new Models.TreatmentRecord
            {
                DiagnosisRecordId = diagnoseId,
                DoctorId = inputModel.doctorId,
                PatientId = inputModel.patientId,

            };
            var treatmentRecord2 = new TreatmentRecord2
            {
                DiagnoseId = diagnoseId, 
                DiagnoseTime = DateTime.Now,
                Commentstate = 0,
                Selfreported = inputModel.selfReported,
                Presenthis = inputModel.presentHis,
                Anamnesis = inputModel.anamnesis,
                Sign = inputModel.sign,
                Clinicdia = inputModel.clinicDia,
                Advice = inputModel.advice,
                Kindquantity = 0//初始等于0，后面解析药品的时候加
            };
            try
            {
                // 查找匹配的挂号记录
                var registration = _context.Registrations.FirstOrDefault(r =>
                    r.PatientId == inputModel.patientId &&
                    r.DoctorId == inputModel.doctorId &&
                    r.AppointmentTime.Date == DateTime.Now.Date &&
                    r.Period == inputModel.period &&
                    r.State == 0
                    );
               
                    // 删除旧的记录
                    _context.Registrations.Remove(registration);

                    // 添加新的记录
                    //因为State是主码的一部分，不可直接修改，必须先删除再插入
                    var newRegistration = new Registration
                    {
                        PatientId= inputModel.patientId,
                        DoctorId= inputModel.doctorId,
                        AppointmentTime = DateTime.Now,
                        Period = inputModel.period,
                        Registorder=registration.Registorder,
                        State = 1,//挂号表中改成已就诊
                        Prescriptionid = prescriptionId,//加入处方编号
                        Checkin=1,
                        Qrcodeurl=registration.Qrcodeurl
                    };


                _context.TreatmentRecords.Add(treatmentRecord);
                _context.TreatmentRecord2s.Add(treatmentRecord2);

                // 解析药品信息
                var medicines = inputModel.medicine.Split(';');//；分割不同的药
                foreach (var medicine in medicines)
                {
                    var medicineInfo = medicine.Split('+');//+分割药品和注意事项

                    if (medicineInfo.Length != 2)
                    {
                        continue;
                    }

                    var medicineNameAndQuantity = medicineInfo[0].Split('*');//*分割药品名称和数量
                    if (medicineNameAndQuantity.Length != 2)
                    {
                        continue;
                    }

                    var medicineName = medicineNameAndQuantity[0];
                    if (!int.TryParse(medicineNameAndQuantity[1], out int quantity))
                    {
                        continue;
                    }
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
                        MedicinePrice = medicineSell.SellingPrice,
                        Quantity = quantity
                    };

                    totalprice += medicineSell.SellingPrice * quantity;
                    treatmentRecord2.Kindquantity += 1;

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

                _context.Prescriptions.Add(prescription);
                _context.Registrations.Add(newRegistration);


                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Treatment record created successfully.");
        }


    }

    public class TreatmentRecordModel
    {
        public string patientId { get; set; }
        public string doctorId { get; set; }
        public int period { get; set; }
        public string selfReported { get; set; }
        public string presentHis { get; set; }
        public string anamnesis { get; set; }
        public string sign { get; set; }
        public string clinicDia { get; set; }
        public string advice { get; set; }
        public string medicine { get; set; }
    }


}
