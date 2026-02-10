using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.DTOs;
using API.Extensions;
using Microsoft.AspNetCore.RateLimiting;
namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserActivityController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        public UserActivityController(IUnitOfWork repo)
        {
            _repo = repo;
        }
        //get
        [Authorize]
        [EnableRateLimiting("read_limit")]
        [HttpGet("saved-library")]
        public async Task<IActionResult> GetMySavedDocuments()
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            var docs = await _repo.userActivityRepo.GetSavedDocumentsByUserAsync(userId);
            return Ok(docs);
        }
        //post
        [Authorize]
        [EnableRateLimiting("export_file_light")]
        [HttpPost("vote/{docId}")]
        public async Task<IActionResult> VoteDocument(int docId, [FromBody] ReqVoteDto dto)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            var result = await _repo.userActivityRepo.VoteDocumentAsync(userId, docId, dto.IsLike);

            if (result) return Ok(new { message = "Đã ghi nhận tương tác." });
            return BadRequest(new { message = "Không thể thực hiện thao tác." });
        }
        [Authorize]
        [EnableRateLimiting("export_file_light")]
        [HttpPost("save/{docId}")]
        public async Task<IActionResult> ToggleSaveDocument(int docId)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            bool result = await _repo.userActivityRepo.ToggleSaveDocumentAsync(userId, docId);
            if(result == false)
            {
                return BadRequest(new { message = "Không thể thực hiện thao tác." });
            }
            return Ok(new { message = "Lưu tài liệu thành công" });
        }
    }
}

