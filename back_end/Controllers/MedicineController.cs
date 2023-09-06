using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicineController : ControllerBase
    {
        private readonly ModelContext _context;

        public MedicineController(ModelContext context)
        {
            _context = context;
        }
        [HttpGet("GetAllMedicinesInfo")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllMedicinesInfo()
        {
            return await _context.MedicineStocks
                .Where(stock => stock.MedicineAmount != 0)
                .Join(_context.MedicineSells,
                    stock => new { stock.MedicineName, stock.Manufacturer },
                    sell => new { sell.MedicineName, sell.Manufacturer },
                    (stock, sell) => new { stock.MedicineName, stock.Manufacturer, sell.SellingPrice })
                .Join(_context.MedicineDescriptions,
                    temp => temp.MedicineName,
                    desc => desc.MedicineName,
                    (temp, desc) => new
                    {
                        temp.MedicineName,
                        temp.Manufacturer,
                        temp.SellingPrice,
                        desc.MedicineType,
                        desc.ApplicableSymptom,
                        desc.Vulgo,
                        desc.Specification,
                        desc.Singledose,
                        desc.Administration,
                        desc.Attention,
                        desc.Frequency
                    })
                .Distinct()
                .ToListAsync();
        }


        [HttpPut("UpdateStock")]//更新库存
        public async Task<IActionResult> UpdateStock(string medicineName, string manufacturer, DateTime productionDate, decimal newAmount, string patientId)
        {
            var medicineStock = await _context.MedicineStocks.FirstOrDefaultAsync(m => m.MedicineName == medicineName && m.Manufacturer == manufacturer && m.ProductionDate.Date == productionDate.Date && m.CleanDate == null);
            if (medicineStock == null)
            {
                return NotFound("未找到该药品");
            }

            // 计算购买数量
            decimal purchaseAmount = (decimal)(medicineStock.MedicineAmount - newAmount);

            // 更新药品库存
            medicineStock.MedicineAmount = newAmount;

            // 创建新的MedicineOut对象
            var medicineOut = new MedicineOut
            {
                MedicineName = medicineName,
                Manufacturer = manufacturer,
                ProductionDate = productionDate,
                PurchaseAmount = purchaseAmount,
                DeliverDate = DateTime.Now, 
                PatientId = patientId
            };

            // 将新对象添加到数据库中
            _context.MedicineOuts.Add(medicineOut);

            await _context.SaveChangesAsync();

            return Ok("Medicine stock and purchase record updated successfully.");
        }


        [HttpPost("AddStock")]//采购入库
        public async Task<IActionResult> AddStock(AddStockInputModel inputModel)
        {
            var existingMedicineStock = await _context.MedicineStocks.FirstOrDefaultAsync(m =>//先检查一下这个药以前有没
                m.MedicineName == inputModel.MedicineName &&
                m.Manufacturer == inputModel.Manufacturer &&
                m.ProductionDate.Date == inputModel.ProductionDate.Date &&
                m.MedicineShelflife == inputModel.MedicineShelflife&&
                m.CleanDate==null
                );

            if (existingMedicineStock != null)
            {
                existingMedicineStock.MedicineAmount += inputModel.MedicineAmount;
                await _context.SaveChangesAsync();
            }
            else
            {
                var medicineStock = new MedicineStock//库存表
                {
                    MedicineName = inputModel.MedicineName,
                    Manufacturer = inputModel.Manufacturer,
                    ProductionDate = inputModel.ProductionDate,
                    MedicineShelflife = inputModel.MedicineShelflife,
                    MedicineAmount = inputModel.MedicineAmount,
                    ThresholdValue = inputModel.ThresholdValue
                };

                _context.MedicineStocks.Add(medicineStock);
            }

            var medicinePurchase = new MedicinePurchase
            {
                MedicineName = inputModel.MedicineName,
                Manufacturer = inputModel.Manufacturer,
                ProductionDate = inputModel.ProductionDate,
                PurchaseDate = DateTime.Now,
                AdministratorId = inputModel.AdministratorId,
                PurchaseAmount = inputModel.MedicineAmount,
                PurchasePrice = inputModel.PurchasePrice
            };

            _context.MedicinePurchases.Add(medicinePurchase);
            await _context.SaveChangesAsync();

            var existingMedicineDescription = await _context.MedicineDescriptions.FirstOrDefaultAsync(m => m.MedicineName == inputModel.MedicineName);

            if (existingMedicineDescription == null)
            {
                var medicineDescription = new MedicineDescription
                {
                    MedicineName = inputModel.MedicineName,
                    MedicineType = inputModel.MedicineType,
                    ApplicableSymptom = inputModel.ApplicableSymptom,
                    Specification = inputModel.Specification,
                    Singledose = inputModel.Singledose,
                    Administration = inputModel.Administration,
                    Attention = inputModel.Attention,
                    Frequency = inputModel.Frequency
                };

                _context.MedicineDescriptions.Add(medicineDescription);
                await _context.SaveChangesAsync();
            }
            var sellInfo = await _context.MedicineSells.FirstOrDefaultAsync(m => m.MedicineName == inputModel.MedicineName && m.Manufacturer == inputModel.Manufacturer);
            if(sellInfo == null)
            {
                var medicineSell = new MedicineSell
                {
                    Manufacturer = inputModel.Manufacturer,
                    MedicineName = inputModel.MedicineName,
                    SellingPrice = (decimal)inputModel.Sellingprice
                };
                _context.MedicineSells.Add(medicineSell);
            }
            await _context.SaveChangesAsync();

            return Ok("Medicine stock added successfully.");
        }

        [HttpPut("CleanMedicine")]
        public async Task<IActionResult> CleanMedicine(string medicineName, string manufacturer, DateTime productionDate, string administratorId)
        {
            var medicineStock = await _context.MedicineStocks.FirstOrDefaultAsync(m =>
                m.MedicineName == medicineName &&
                m.Manufacturer == manufacturer &&
                m.ProductionDate.Date == productionDate.Date);


            if (medicineStock == null)
            {
                return NotFound("未找到该药品库存");
            }

            medicineStock.CleanDate = DateTime.Now.Date;
            medicineStock.CleanAdministrator = administratorId;
            medicineStock.MedicineAmount = 0;

            var allStocks = await _context.MedicineStocks.Where(m => m.MedicineName == medicineName && m.Manufacturer == manufacturer).ToListAsync();
            if (allStocks.All(m => m.CleanDate != null))//该厂家的同款药都被清理了，就下架
            {
                var medicineSell = await _context.MedicineSells.FirstOrDefaultAsync(m => m.MedicineName == medicineName && m.Manufacturer == manufacturer);
                if (medicineSell != null)
                {
                    _context.MedicineSells.Remove(medicineSell);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Medicine cleaned successfully.");
        }


        [HttpGet("GetCleanedMedicines")]
        public IActionResult GetCleanedMedicines()
        {
            var cleanedMedicines = _context.MedicineStocks.Where(m => m.CleanDate != null).ToList();

            var result = cleanedMedicines.Select(m =>
            {
                var administrator = _context.Administrators.FirstOrDefault(a => a.AdministratorId == m.CleanAdministrator);

                return new
                {
                    MedicineName = m.MedicineName,
                    CleanDate = m.CleanDate,
                    administratorId=m.CleanAdministrator,
                    CleanAdministrator = administrator?.Name,
                    Manufacturer=m.Manufacturer,
                    Productiondate=m.ProductionDate
                };
            });

            return Ok(result);
        }

        [HttpGet("GetMedicineStatistics")]
        public async Task<IActionResult> GetMedicineStatistics()
        {
            // 在MedicinePurchase表中统计每款药的每月购买数量、购买支出
            var purchaseStatistics = _context.MedicinePurchases
                .GroupBy(mp => new { mp.MedicineName, mp.Manufacturer, mp.PurchaseDate.Year, mp.PurchaseDate.Month })
                .Select(g => new
                {
                    g.Key.MedicineName,
                    g.Key.Manufacturer,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    PurchaseAmount = g.Sum(mp => mp.PurchaseAmount),
                    PurchaseCost = g.Sum(mp => mp.PurchaseAmount * mp.PurchasePrice)
                });

            // 在MedicineOut表中统计每款药的每月销售数量、销售收入
            var sellStatistics = _context.MedicineOuts
                .GroupBy(mo => new { mo.MedicineName, mo.Manufacturer, mo.DeliverDate.Year, mo.DeliverDate.Month })
                .Select(g => new
                {
                    g.Key.MedicineName,
                    g.Key.Manufacturer,
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    SellAmount = g.Sum(mo => mo.PurchaseAmount),
                    SellIncome = g.Sum(mo => mo.PurchaseAmount * _context.MedicineSells.FirstOrDefault(ms => ms.MedicineName == mo.MedicineName && ms.Manufacturer == mo.Manufacturer).SellingPrice)
                });

            //// 将这两个表进行左右自然外连接，用药品名和厂家匹配，外连接时未出现在另一表中的值默认为0
            //var statistics = purchaseStatistics.GroupJoin(sellStatistics,
            //    ps => new { ps.MedicineName, ps.Manufacturer, ps.Year, ps.Month },
            //    ss => new { ss.MedicineName, ss.Manufacturer, ss.Year, ss.Month },
            //    (ps, ss) => new { ps, ss })
            //    .SelectMany(
            //        x => x.ss.DefaultIfEmpty(),
            //        (x, y) => new
            //        {
            //            x.ps.MedicineName,
            //            x.ps.Manufacturer,
            //            x.ps.Year,
            //            x.ps.Month,
            //            PurchaseAmount = x.ps.PurchaseAmount,
            //            PurchaseCost = x.ps.PurchaseCost,
            //            SellAmount = (y==null)?0:y.SellAmount,
            //            SellIncome = (y == null) ? 0 : y.SellIncome
            //        });

            return Ok(new { purchaseStatistics, sellStatistics });

        }


    }

    public class AddStockInputModel
    {

        public string MedicineName { get; set; }
        public string Manufacturer { get; set; }
        public DateTime ProductionDate { get; set; }
        public decimal MedicineShelflife { get; set; }
        public decimal MedicineAmount { get; set; }
        public decimal ThresholdValue { get; set; }
        public string AdministratorId { get; set; }
        public decimal PurchasePrice { get; set; }
        public string MedicineType { get; set; }
        public string ApplicableSymptom { get; set; }
        public string Specification { get; set; }
        public string Singledose { get; set; }
        public string Administration { get; set; }
        public string Attention { get; set; }
        public string Frequency { get; set; }
        public double Sellingprice { get; set; }
    }
}

