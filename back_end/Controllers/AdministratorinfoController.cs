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
                .Select(a => new { a.AdministratorId, a.Name, a.Gender, a.Contact,a.Password })
                .ToListAsync();
            return administrators;
        }

        // 通过id查找管理员
        [HttpGet("id")]
        public async Task<ActionResult<object>> GetAdministratorbyId(string id)
        {
            var administrator = await _context.Administrators
                .Where(a => a.AdministratorId == id)
                .Select(a => new { a.AdministratorId, a.Name, a.Gender, a.Contact,a.Password })
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

        //修改管理员信息
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAdministrator(Administrator administrator)
        {
            if (!AdministratorExists(administrator.AdministratorId))//先检查一下要修改的信息存不存在
            {
                return NotFound();
            }

            _context.Entry(administrator).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();//实现修改
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdministratorExists(administrator.AdministratorId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        private bool AdministratorExists(string id)
        {
            return _context.Administrators.Any(e => e.AdministratorId == id);
        }

    }
}
