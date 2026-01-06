using API.DTOs;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        public UserController(IUnitOfWork repo)
        {
            _repo = repo;
        }
        [HttpGet("user/filedocs")]
        [Authorize]
        public async Task<IActionResult> FileDoc()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Unauthorized("Token không hợp lệ hoặc thiếu thông tin định danh.");
            }
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest("UserId trong Token không đúng định dạng.");
            }
            ResUserStorageFileDto? userStorageInfo = await _repo.usersRepo.GetUserStorageStatsAsync(userId);
            if (userStorageInfo == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }
            return Ok(userStorageInfo);
        }
        [HttpGet("user/privateprofile")]
        [Authorize]
        public async Task<IActionResult> PrivateProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return Unauthorized("Token không hợp lệ hoặc thiếu thông tin định danh.");
            }
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                return BadRequest("UserId trong Token không đúng định dạng.");
            }
            ResUserPrivate? userPrivateProfile = await _repo.usersRepo.GetUserPrivateProfileAsync(userId);
            if (userPrivateProfile == null)
            {
                return NotFound("Không tìm thấy thông tin người dùng.");
            }
            return Ok(userPrivateProfile);
        }
    }
}
