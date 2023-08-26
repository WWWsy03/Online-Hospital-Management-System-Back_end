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

        [HttpGet("GetAllStocks")] 
        public async Task<IActionResult> GetMedicineStocks()
        {
            var stocks = await _context.MedicineStocks.Include(m=>m.CleanAdministratorNavigation)
        .ToListAsync();
            return Ok(stocks);
        }

    }
}
