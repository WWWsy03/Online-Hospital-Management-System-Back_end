using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly ModelContext _context;
        public StockController(ModelContext context) 
        {
            _context=context;
    }


        [HttpGet("GetAllStocks")]//获取所有有库存的药的库存信息
        public async Task<IActionResult> GetMedicineStocks()
        {
            var stocks = await _context.MedicineStocks
                .Include(m => m.CleanAdministratorNavigation)
                .Where(m => m.CleanDate == null)
                .ToListAsync();
            return Ok(stocks);
        }

    }
}
