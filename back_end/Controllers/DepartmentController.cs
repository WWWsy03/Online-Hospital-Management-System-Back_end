using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using back_end.Models;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Department2Controller : ControllerBase
    {
        private readonly ModelContext _context;

        public Department2Controller(ModelContext context)
        {
            _context = context;
        }

        // GET获取所有科室信息
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department2>>> GetDepartments()
        {
            return await _context.Department2s.ToListAsync();
        }
    }
}
