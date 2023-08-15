using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using back_end.Models;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdministratorsController : ControllerBase
    {
        private readonly ModelContext _context;

        public AdministratorsController(ModelContext context)
        {
            _context = context;
        }

        // 获取管理员信息
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAdministrators()
        {
            var administrators = await _context.Administrators
                .Select(a => new { a.AdministratorId, a.Name, a.Gender, a.Contact })
                .ToListAsync();
            return administrators;
        }

        // 通过id查找管理员
        [HttpGet("id")]
        public async Task<ActionResult<object>> GetAdministratorbyId(string id)
        {
            var administrator = await _context.Administrators
                .Where(a => a.AdministratorId == id)
                .Select(a => new { a.AdministratorId, a.Name, a.Gender, a.Contact })
                .FirstOrDefaultAsync();

            if (administrator == null)
            {
                return NotFound();
            }

            return administrator;
        }

        // 输入管理员姓名查找指定管理员信息
        [HttpGet("name")]
        public async Task<ActionResult<IEnumerable<Administrator>>> GetAdministratorbyName(string name)
        {
            var administrator = await _context.Administrators
                .Where(d => d.Name == name)
                .ToListAsync();

            if (administrator == null || administrator.Count == 0)
            {
                return NotFound();
            }

            return administrator;
        }

        // 向数据库中插入一个管理员信息
        [HttpPost("add")]
        public async Task<ActionResult<Administrator>> PostAdministrator(Administrator administrator)
        {
            _context.Administrators.Add(administrator);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAdministrator", new { id = administrator.AdministratorId }, administrator);
        }

    }
}
