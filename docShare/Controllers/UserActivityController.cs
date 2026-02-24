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
    [Route("api/user-activity")]
    [ApiController]
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
            if (result == false)
            {
                return BadRequest(new { message = "Không thể thực hiện thao tác." });
            }
            return Ok(new { message = "Lưu tài liệu thành công" });
        }
        [Authorize]
        [HttpPost("follow")]
        public async Task<IActionResult> FollowUser([FromBody] ReqFollowDto dto)
        {
            int followerId = User.GetUserId();
            if (followerId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            if (followerId == dto.FollowedId) return BadRequest(new { message = "Không thể theo dõi chính mình." });
            bool ishas = await _repo.userActivityRepo.HasFollowedAsync(followerId, dto.FollowedId);
            if (ishas) return BadRequest(new { message = "Đã theo dõi người dùng này." });
            try
            {
                await _repo.userActivityRepo.CreateFollowingAsync(followerId, dto.FollowedId);
                await _repo.SaveAllAsync();
                return Ok(new { message = "Đã theo dõi người dùng." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpDelete("unfollow/{userId}")]
        public async Task<IActionResult> UnfollowUser( int userId)
        {
            int followerId = User.GetUserId();
            if (followerId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            if (followerId == userId) return BadRequest(new { message = "Không thể bỏ theo dõi chính mình." });
            bool ishas = await _repo.userActivityRepo.HasFollowedAsync(followerId, userId);
            if (!ishas) return BadRequest(new { message = "Chưa theo dõi người dùng này." });
            try
            {
                await _repo.userActivityRepo.RemoveFollowingAsync(followerId, userId);
                await _repo.SaveAllAsync();
                return Ok(new { message = "Đã bỏ theo dõi người dùng." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }
    }
}

