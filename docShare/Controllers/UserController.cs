using API.DTOs;
using API.Extensions;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly IStorageService _s3storage;
        public UserController(IUnitOfWork repo, IStorageService s3storage)
        {
            _repo = repo;
            _s3storage = s3storage;
        }
        // get
        [HttpGet("user/storageDoc")]
        [Authorize]
        [EnableRateLimiting("read_limit")]
        public async Task<IActionResult> FileDoc()
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
            ResUserStorageFileDto? userStorageInfo = await _repo.usersRepo.GetUserStorageStatsAsync(userId);
            if (userStorageInfo == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });
            }
            return Ok(userStorageInfo);
        }
        [HttpGet("user/privateprofile")]
        [Authorize]
        [EnableRateLimiting("read_limit")]

        public async Task<IActionResult> PrivateProfile()
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
            ResUserPrivate? userPrivateProfile = await _repo.usersRepo.GetUserPrivateProfileAsync(userId);
            if (userPrivateProfile == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });
            }
            return Ok(userPrivateProfile);
        }
        //patch
        [HttpPatch("user/update/profile")]
        [Authorize]
        [EnableRateLimiting("export_file_light")]
        public async Task<IActionResult> UpdatePrivateProfile(ReqUserUpdateDto reqUserUpdateDto)
        {
            try
            {

                int userId = User.GetUserId();
                bool ishasuser = await _repo.usersRepo.HasUser(userId);
                if (userId == 0 || !ishasuser) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
                await _repo.usersRepo.UpdateUserProfile(userId, reqUserUpdateDto.Email, reqUserUpdateDto.Password
                    , reqUserUpdateDto.FullName);
                await _repo.SaveAllAsync();
                var updatedProfile = await _repo.usersRepo.GetUserPrivateProfileAsync(userId);
                return Ok(new
                {
                    data = updatedProfile
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpPatch("user/update/avatar")]
        [Authorize]
        [EnableRateLimiting("upload_limit")]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatar)
        {
            int userId = User.GetUserId();
            string? avatarFileName = null;
            bool ishasuser = await _repo.usersRepo.HasUser(userId);
            if (userId == 0 || !ishasuser) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
            try
            {

                if (avatar != null && avatar.Length > 0)
                {
                    var ext = Path.GetExtension(avatar.FileName);
                    avatarFileName = $"{userId}_{ext}";
                    using var stream = avatar.OpenReadStream();
                    await _s3storage.UploadFileAsync(stream, avatarFileName, avatar.ContentType, StorageType.Avatar);
                    await _repo.usersRepo.UpdateUserAvatar(userId, avatarFileName);
                    await _repo.SaveAllAsync();
                    return Ok(new { data = avatarFileName });
                }
                return BadRequest(new { message = "Cập nhật thông tin người dùng thất bại." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpPatch("user/update/username")]
        [Authorize]
        [EnableRateLimiting("export_file_light")]
        public async Task<IActionResult> UpdateUsername(ReqUpdateUserNameDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Username))
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }
            int userId = User.GetUserId();
            bool ishasuser = await _repo.usersRepo.HasUser(userId);
            if (userId == 0 || !ishasuser) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
            try
            {

                bool ishasname = await _repo.usersRepo.ExistUserNameAsync(dto.Username);
                if (ishasname)
                {
                    return Conflict(new { message = "Username đã tồn tại." });
                }
                await _repo.usersRepo.UpdateUserNameAsync(dto.Username, userId);
                await _repo.SaveAllAsync();
                return Ok(new { date = dto.Username });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpPatch("user/update/password")]
        [Authorize]
        [EnableRateLimiting("export_file_light")]
        public async Task<IActionResult> UpdatePassword([FromBody] ReqUpdatePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = User.GetUserId();
            bool ishasuser = await _repo.usersRepo.HasUser(userId);
            if (userId == 0 || !ishasuser) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {
                string? currentPasswordHash = await _repo.usersRepo.GetPasswordByUserId(userId);
                if (!string.IsNullOrEmpty(currentPasswordHash))
                {
                    if (string.IsNullOrEmpty(dto.OldPassword))
                    {
                        return BadRequest(new { message = "Vui lòng nhập mật khẩu cũ." });
                    }
                    bool checkPassword = BCrypt.Net.BCrypt.Verify(dto.OldPassword, currentPasswordHash);
                    if (!checkPassword)
                    {
                        return BadRequest(new { message = "Mật khẩu cũ không chính xác." });
                    }
                    if (dto.OldPassword == dto.NewPassword)
                    {
                        return BadRequest(new { message = "Mật khẩu mới không được trùng với mật khẩu cũ." });
                    }
                }
                else
                {
                }
                await _repo.usersRepo.UpdateUserPassword(dto.NewPassword, userId);
                await _repo.SaveAllAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server nội bộ." });
            }
        }
    }
}
