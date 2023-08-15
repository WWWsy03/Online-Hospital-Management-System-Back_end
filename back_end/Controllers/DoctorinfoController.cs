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
    public class DoctorsController : ControllerBase
    {
        private readonly ModelContext _context;

        public DoctorsController(ModelContext context)
        {
            _context = context;
        }

        // 获取医生信息表中的所有医生数据
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctors()
        {
            return await _context.Doctors.ToListAsync();
        }


        // 输入医生id查找指定医生信息
        [HttpGet("id")]
        public async Task<ActionResult<Doctor>> GetDoctorbyId(string id)
        {
            var doctor = await _context.Doctors.FindAsync(id);

            if (doctor == null)
            {
                return NotFound();
            }

            return doctor;
        }


        // 输入医生姓名查找指定医生信息
        [HttpGet("name")]
        public async Task<ActionResult<IEnumerable<Doctor>>> GetDoctorbyName(string name)
        {
            var doctors = await _context.Doctors
                .Where(d => d.Name == name)
                .ToListAsync();

            if (doctors == null || doctors.Count == 0)
            {
                return NotFound();
            }

            return doctors;
        }


        //插入医生信息
        [HttpPost("adddoctor")]
        public async Task<ActionResult<Doctor>> PostDoctor(Doctor doctor)
        {
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDoctor", new { id = doctor.DoctorId }, doctor);
        }

    }
}
