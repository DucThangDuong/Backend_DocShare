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
        [EnableRateLimiting("read_standard")]
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
        [EnableRateLimiting("write_standard")]
        [HttpPost("vote/{docId}")]
        public async Task<IActionResult> VoteDocument(int docId, [FromBody] ReqVoteDto dto)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {

                await _repo.userActivityRepo.AddVoteDocumentAsync(userId, docId, dto.IsLike);
                await _repo.SaveAllAsync();
                return Ok(new { message = "Đã ghi nhận tương tác." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [EnableRateLimiting("write_standard")]
        [HttpPost("save/{docId}")]
        public async Task<IActionResult> SaveDocument(int docId )
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {
                await _repo.userActivityRepo.AddUserSaveDocumentAsync(userId, docId);
                await _repo.SaveAllAsync();
                return Ok(new { message = "Lưu tài liệu thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [Authorize]
        [HttpPost("follow")]
        [EnableRateLimiting("write_standard")]
        public async Task<IActionResult> FollowUser([FromBody] ReqFollowDto dto)
        {
            int followerId = User.GetUserId();
            if (followerId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            if (followerId == dto.FollowedId) return BadRequest(new { message = "Không thể theo dõi chính mình." });
            bool ishas = await _repo.userActivityRepo.HasFollowedAsync(followerId, dto.FollowedId);
            if (ishas) return BadRequest(new { message = "Đã theo dõi người dùng này." });
            try
            {
                _repo.userActivityRepo.AddFollowing(followerId, dto.FollowedId);
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
        [EnableRateLimiting("write_standard")]
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

