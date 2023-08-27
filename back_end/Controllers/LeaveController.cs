﻿using back_end.Models;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace back_end.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeaveController : ControllerBase
    {
        private readonly ModelContext _context;
        //private static readonly object _lock = new object();

        public LeaveController(ModelContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<LeaveApplication>> PostLeaveApplication(string diagnosedId,int leaveDays )
        {
            if (leaveDays == 0)
            {
                return Ok("No need for leave");
            }
            // 生成假条ID
            var leaveNoteId = diagnosedId + leaveDays.ToString("D3");

            // 获取请假开始日期
            var leaveStartDate = DateTime.ParseExact(diagnosedId.Substring(0, 8), "yyyyMMdd", CultureInfo.InvariantCulture);

            // 创建假条申请记录
            var leaveApplication = new LeaveApplication
            {
                LeaveNoteId = leaveNoteId,
                LeaveApplicationTime = DateTime.Now,
                LeaveStartDate = leaveStartDate,
                LeaveEndDate = leaveStartDate.AddDays(leaveDays),
                LeaveNoteRemark = "未审核"
            };

            try
            {
                _context.LeaveApplications.Add(leaveApplication);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Leave application created successfully.");
        }

    }
}