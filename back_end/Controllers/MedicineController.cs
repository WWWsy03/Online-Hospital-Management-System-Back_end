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
        [HttpGet]
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

    }
}
