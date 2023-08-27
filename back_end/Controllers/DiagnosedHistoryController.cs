using back_end.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosedHistoryController : ControllerBase
    {
        private readonly ModelContext _context;

        public DiagnosedHistoryController(ModelContext context)
        {
            _context = context;
        }
        [HttpGet("getPatientRecords")]
        public async Task<ActionResult<IEnumerable<TreatmentRecord2>>> GetPatientRecords(string patientId)
        {
            // 查找与病人ID匹配的所有诊断记录ID
            var diagnosisRecordIds = await _context.TreatmentRecords
                .Where(r => r.PatientId == patientId)
                .Select(r => r.DiagnosisRecordId)
                .ToListAsync();

            // 使用这些诊断记录ID在TreatmentRecord2表中查询每个ID对应的属性
            var patientRecords = await _context.TreatmentRecord2s
                .Where(r => diagnosisRecordIds.Contains(r.DiagnoseId))
                .ToListAsync();

            return Ok(patientRecords);
        }

    }
}
