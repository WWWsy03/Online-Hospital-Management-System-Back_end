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
        public async Task<ActionResult<IEnumerable<object>>> GetMedicines()
        {
            return await _context.MedicineDescriptions
                .Join(_context.MedicineSells,
                    desc => desc.MedicineName,
                    sell => sell.MedicineName,
                    (desc, sell) => new
                    {
                        desc.MedicineName,
                        desc.MedicineType,
                        desc.ApplicableSymptom,
                        desc.Vulgo,
                        desc.Specification,
                        desc.Singledose,
                        desc.Administration,
                        desc.Attention,
                        desc.Frequency,
                        sell.Manufacturer,
                        sell.SellingPrice
                    })
                .ToListAsync();
        }

        [HttpPut("UpdateStock")]//更新库存
        public async Task<IActionResult> UpdateStock(string medicineName, decimal newAmount)
        {
            var medicineStock = await _context.MedicineStocks.FirstOrDefaultAsync(m => m.MedicineName == medicineName);
            if (medicineStock == null)
            {
                return NotFound("未找到该药品");
            }

            medicineStock.MedicineAmount = newAmount;

            await _context.SaveChangesAsync();

            return Ok("Medicine stock updated successfully.");
        }

        [HttpPost("AddStock")]
        public async Task<IActionResult> AddStock(AddStockInputModel inputModel)
        {
            var medicineStock = new MedicineStock
            {
                MedicineName = inputModel.MedicineName,
                Manufacturer = inputModel.Manufacturer,
                ProductionDate = inputModel.ProductionDate,
                MedicineShelflife = inputModel.MedicineShelflife,
                MedicineAmount = inputModel.MedicineAmount,
                ThresholdValue = inputModel.ThresholdValue
            };

            _context.MedicineStocks.Add(medicineStock);

            await _context.SaveChangesAsync();

            return Ok("Medicine stock added successfully.");
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
                    CleanAdministrator = administrator?.Name
                };
            });

            return Ok(result);
        }
    }

    public class AddStockInputModel
    {
        public string MedicineName { get; set; }
        public string Manufacturer { get; set; }
        public DateTime ProductionDate { get; set; }
        public decimal MedicineShelflife { get; set; }
        public decimal? MedicineAmount { get; set; }
        public decimal ThresholdValue { get; set; }
    }

}

