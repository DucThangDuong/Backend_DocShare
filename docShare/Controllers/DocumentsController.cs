using API.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.DTOs;

namespace API.Controllers
{
    [Route("api")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _repo;
        public DocumentsController(IConfiguration config,IUnitOfWork repo)
        {
            _config = config;
            _repo = repo;
        }
        [HttpPost("document")]
        [Authorize] 
        public async Task<IActionResult> UploadDocument([FromForm] ReqCreateDocumentDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("Vui lòng chọn file.");

            if (Path.GetExtension(dto.File.FileName).ToLower() != ".pdf")
                return BadRequest("Chỉ chấp nhận file PDF.");
            try
            {
                string storagePath = _config["FileStorage:UploadFolderPath"]! ;

                if (!Directory.Exists(storagePath))
                    Directory.CreateDirectory(storagePath);

                string uniqueFileName = $"{Path.GetFileNameWithoutExtension(dto.File.FileName)}" +
                    $"^_{Guid.NewGuid()}{Path.GetExtension(dto.File.FileName)}";

                string fullPathOnDisk = Path.Combine(storagePath, uniqueFileName);
                using (var stream = new FileStream(fullPathOnDisk, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if(userId == null)
                {
                    return Unauthorized();
                }
                var newDoc = new Document
                {
                    Title = dto.Title,
                    FileUrl = fullPathOnDisk,
                    SizeInBytes = dto.File.Length,
                    UploaderId = int.Parse(userId!),
                    Status = dto.Status,
                    IsDeleted = 0,
                    CreatedAt = DateTime.UtcNow
                };
                bool iscreate=await _repo.documentsRepo.CreateAsync(newDoc);
                if (iscreate)
                {
                    return Created();
                }
                return BadRequest(new
                {
                    message = "Lỗi khi tải dữ liệu"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
        [HttpGet("documents")]
        [Authorize]
        public async Task<IActionResult> GetInforDoc([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            if (take > 50) { take = 50; }
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized();
            }
            try
            {
                List<ResDocumentDto> response = await _repo.documentsRepo.GetDocsByUserIdPagedAsync(userId, skip, take);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest("Có lỗi xảy ra khi tải dữ liệu.");
            }
        }
    }
}
