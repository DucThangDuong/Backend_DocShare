using API.DTOs;
using API.Extensions;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly IStorageService _s3storage;
        private readonly IConfiguration _config;
        public UserController(IUnitOfWork repo, IStorageService s3storage, IConfiguration config)
        {
            _repo = repo;
            _s3storage = s3storage;
            _config = config;
        }
        // get
        [HttpGet("me/storage")]
        [Authorize]
        [EnableRateLimiting("read_standard")]
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
        [HttpGet("me/profile")]
        [Authorize]
        [EnableRateLimiting("read_standard")]
        public async Task<IActionResult> PrivateProfile()
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
            ResUserPrivate? userPrivateProfile = await _repo.usersRepo.GetUserPrivateProfileAsync(userId);
            if (userPrivateProfile == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });
            }
            userPrivateProfile.avatarUrl = StringHelpers.GetFinalAvatarUrl(userPrivateProfile.avatarUrl ?? "");
            return Ok(userPrivateProfile);
        }
        [HttpGet("me/documents/save")]
        [EnableRateLimiting("read_standard")]
        [Authorize]
        public async Task<IActionResult> DocumentsUserSave() {
            int currentId = User.GetUserId();
            if (currentId <= 0) return BadRequest(new { message = "ID người dùng không hợp lệ." });
            List<ResSummaryDocumentDto>? result = await _repo.documentsRepo.GetDocumentSaveOfUser(currentId);
            return Ok(result);
        }
        [HttpGet("me/documents/like")]
        [EnableRateLimiting("read_standard")]
        [Authorize]
        public async Task<IActionResult> DocumentsUserLike()
        {
            int currentId = User.GetUserId();
            if (currentId <= 0) return BadRequest(new { message = "ID người dùng không hợp lệ." });
            List<ResSummaryDocumentDto>? result = await _repo.documentsRepo.GetDocumentLikeOfUser(currentId);
            return Ok(result);
        }
        [HttpGet("me/documents/upload")]
        [EnableRateLimiting("read_standard")]
        [Authorize]
        public async Task<IActionResult> DocumentsUserUpload()
        {
            int currentId = User.GetUserId();
            if (currentId <= 0) return BadRequest(new { message = "ID người dùng không hợp lệ." });
            List<ResSummaryDocumentDto>? result = await _repo.documentsRepo.GetDocumentUploadOfUser(currentId);
            return Ok(result);
        }

        [HttpGet("{userId}/profile")]
        [EnableRateLimiting("read_standard")]
        public async Task<IActionResult> PublicProfile(int userId)
        {
            int currentId = User.GetUserId();
            if (userId <= 0) return BadRequest(new { message = "ID người dùng không hợp lệ." });
            ResUserPublicDto? userPublicProfile = await _repo.usersRepo.GetUserPublicProfileAsync(userId,currentId);
            if (userPublicProfile == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin người dùng." });
            }
            userPublicProfile.avatarUrl = StringHelpers.GetFinalAvatarUrl(userPublicProfile.avatarUrl ?? "");
            return Ok(userPublicProfile);
        }
        [HttpGet("{userId}/documents")]
        [EnableRateLimiting("read_standard")]
        public async Task<IActionResult> GetUserDocuments(int userId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            if (take > 50) { take = 50; }
            bool ishas = await _repo.usersRepo.HasValue(userId);
            if (!ishas) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {
                List<ResDocumentDetailEditDto> response = await _repo.documentsRepo.GetDocsByUserIdPagedAsync(userId, skip, take);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return BadRequest(new { message = "Có lỗi xảy ra khi tải dữ liệu." });
            }
        }
        [HttpGet("{userId}/stats")]
        [EnableRateLimiting("read_standard")]
        public async Task<IActionResult> GetUserStats(int userId)
        {
            bool ishas = await _repo.usersRepo.HasValue(userId);
            if (!ishas) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            ResUserStatsDto? userStatsDto = await _repo.documentsRepo.GetUserStatsAsync(userId);
            if (userStatsDto == null)
            {
                userStatsDto = new ResUserStatsDto
                {
                    SavedCount = 0,
                    UploadCount = 0,
                    TotalLikesReceived = 0
                };
            }
            return Ok(userStatsDto);
        }
        //patch
        [HttpPatch("me/profile")]
        [Authorize]
        [EnableRateLimiting("write_standard")]
        public async Task<IActionResult> UpdatePrivateProfile(ReqUserUpdateDto reqUserUpdateDto)
        {
            try
            {
                int userId = User.GetUserId();
                bool ishasuser = await _repo.usersRepo.HasValue(userId);
                if (userId == 0 || !ishasuser) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
                await _repo.usersRepo.UpdateUserProfile(userId, reqUserUpdateDto.Email, reqUserUpdateDto.Password
                    , reqUserUpdateDto.FullName,reqUserUpdateDto.UniversityId);
                await _repo.SaveAllAsync();
                ResUserPrivate? updatedProfile = await _repo.usersRepo.GetUserPrivateProfileAsync(userId);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpPatch("me/avatar")]
        [Authorize]
        [EnableRateLimiting("write_heavy")]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatar)
        {
            int userId = User.GetUserId();
            string? avatarFileName = null;
            bool ishasuser = await _repo.usersRepo.HasValue(userId);
            if (userId == 0 || !ishasuser) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
            try
            {

                if (avatar != null && avatar.Length > 0)
                {
                    var ext = Path.GetExtension(avatar.FileName);
                    avatarFileName = StringHelpers.Create_s3ObjectKey_avatar(ext, userId);
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
        [HttpPatch("me/username")]
        [Authorize]
        [EnableRateLimiting("write_standard")]
        public async Task<IActionResult> UpdateUsername(ReqUpdateUserNameDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Username))
            {
                return BadRequest(new { message = "Dữ liệu không hợp lệ." });
            }
            int userId = User.GetUserId();
            bool ishasuser = await _repo.usersRepo.HasValue(userId);
            if (userId == 0 || !ishasuser) return Unauthorized(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." });
            try
            {

                bool ishasname = await _repo.usersRepo.HasUserNameAsync(dto.Username);
                if (ishasname)
                {
                    return Conflict(new { message = "Username đã tồn tại." });
                }
                await _repo.usersRepo.UpdateUserNameAsync(dto.Username, userId);
                await _repo.SaveAllAsync();
                return Ok(new { data = dto.Username });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpPatch("me/password")]
        [Authorize]
        [EnableRateLimiting("write_standard")]
        public async Task<IActionResult> UpdatePassword([FromBody] ReqUpdatePasswordDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            int userId = User.GetUserId();
            bool ishasuser = await _repo.usersRepo.HasValue(userId);
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
