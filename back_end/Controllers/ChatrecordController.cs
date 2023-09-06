using back_end.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace back_end.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatrecordController : Controller
    {
        private readonly ModelContext _context;

        public ChatrecordController(ModelContext context)
        {
            _context = context;
        }
        [HttpGet("getChatRecord")]
        public async Task<ActionResult<IEnumerable<Chatrecord>>> getChatRecord(string RecordId)
        {
            var chatRecords = _context.Chatrecords.Where(r => r.Recordid == RecordId).OrderBy(r => r.Timestamp).ToArray();
            if (chatRecords == null || !chatRecords.Any())
            {
                return NotFound("No records found.");
            }

            return Ok(chatRecords);
        }

        [HttpPost("addChatRecord")]
        public async Task<ActionResult<string>> addChatRecord(ChatRecordInputModel addedChatRecord)
        {
            var oldChatRecord = _context.Chatrecords.Any(r => r.PatientId == addedChatRecord.PatientId
                && r.DoctorId == addedChatRecord.DoctorId
                && r.Timestamp == addedChatRecord.TimeStamp);
            if (oldChatRecord == true)
            {
                return BadRequest("This message has already existed!");
            }
            var newChatRecord = new Chatrecord()
            {
                Recordid = addedChatRecord.RecordId,
                PatientId = addedChatRecord.PatientId,
                DoctorId = addedChatRecord.DoctorId,
                Message = addedChatRecord.Message,
                SenderType = addedChatRecord.SenderType,
                Timestamp = addedChatRecord.TimeStamp,
                ReadStatus = addedChatRecord.ReadStatus
            };

            newChatRecord.Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == addedChatRecord.DoctorId);
            newChatRecord.Patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == addedChatRecord.PatientId);
            _context.Chatrecords.Add(newChatRecord);
            await _context.SaveChangesAsync();
            return Ok("New message added successfully!");
        }
    }

    public class ChatRecordInputModel
    {
        public string RecordId { get; set; }
        public string DoctorId { get; set; }
        public string PatientId { get; set; }
        public string Message { get; set; }
        public int SenderType { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ReadStatus { get; set; }
    }
}
