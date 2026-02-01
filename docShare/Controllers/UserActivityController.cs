using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using API.DTOs;
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
        [EnableRateLimiting("export_file_light")]
        [HttpPost("vote/{docId}")]
        public async Task<IActionResult> VoteDocument(int docId, [FromBody] ReqVoteDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Không xác định được danh tính người dùng.");
            }
            var result = await _repo.userActivityRepo.VoteDocumentAsync(userId, docId, dto.IsLike);

            if (result) return Ok(new { Message = "Đã ghi nhận tương tác." });
            return BadRequest(new { message = "Không thể thực hiện thao tác." });
        }
        [HttpPost("save/{docId}")]
        public async Task<IActionResult> ToggleSaveDocument(int docId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Không xác định được danh tính người dùng.");
            }
            bool result = await _repo.userActivityRepo.ToggleSaveDocumentAsync(userId, docId);
            if(result == false)
            {
                return BadRequest(new { message = "Không thể thực hiện thao tác." });
            }
            return Ok(new { Message = "Lưu tài liệu thành công" });
        }
        [HttpGet("saved-library")]
        public async Task<IActionResult> GetMySavedDocuments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Không xác định được danh tính người dùng.");
            }
            var docs = await _repo.userActivityRepo.GetSavedDocumentsByUserAsync(userId);
            return Ok(docs);
        }
    }
}

