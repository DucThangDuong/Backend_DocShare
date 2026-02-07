using API.DTOs;
using API.Services;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using API.Extensions;
namespace API.Controllers
{
    [Route("api")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IUnitOfWork _repo;
        private readonly RabbitMQService _rabbitMQService;
        private readonly IStorageService _storageService;
        public DocumentsController(IConfiguration config, IUnitOfWork repo, RabbitMQService rabbitMQService, IStorageService storageService)
        {
            _config = config;
            _repo = repo;
            _rabbitMQService = rabbitMQService;
            _storageService = storageService;
        }
        [HttpPost("document/check")]
        [Authorize]
        public async Task<IActionResult> CheckDocument([FromForm] ReqCreateDocumentDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file." });
            if (Path.GetExtension(dto.File.FileName).ToLower() != ".pdf")
                return BadRequest(new { message = "Chỉ chấp nhận file PDF." });

            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {
                string s3ObjectKey = StringHelpers.Create_s3ObjectKey(dto.File.FileName, userId);
                bool isExist = await _storageService.FileExistsAsync(s3ObjectKey, StorageType.Document);
                if (isExist)
                {
                    return BadRequest($"File '{dto.File.FileName}' đã tồn tại trên hệ thống. Vui lòng đổi tên hoặc kiểm tra lại.");
                }
                using (var stream = dto.File.OpenReadStream())
                {
                    await _storageService.UploadFileAsync(stream, s3ObjectKey, "application/pdf", StorageType.Document);
                }
                await _rabbitMQService.SendFileToScan(s3ObjectKey, $"{userId}", $"{dto.Title}");
                return Ok(new
                {
                    message = "File đã được tải lên và đang chờ quét.",
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }

        [HttpPost("document")]
        [Authorize]
        [EnableRateLimiting("upload_limit")]
        public async Task<IActionResult> UploadDocument([FromForm] ReqCreateDocumentDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file." });

            if (Path.GetExtension(dto.File.FileName).ToLower() != ".pdf")
                return BadRequest(new { message = "Chỉ chấp nhận file PDF." });
            try
            {
                int userId = User.GetUserId();
                if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                string s3ObjectKey = StringHelpers.Create_s3ObjectKey(dto.File.FileName, userId);
                bool isExist = await _storageService.FileExistsAsync(s3ObjectKey, StorageType.Document);
                if (isExist)
                {
                    return BadRequest($"File '{dto.File.FileName}' đã tồn tại trên hệ thống. Vui lòng đổi tên hoặc kiểm tra lại.");
                }
                using (var stream = dto.File.OpenReadStream())
                {
                    await _storageService.UploadFileAsync(stream, s3ObjectKey, "application/pdf", StorageType.Document);
                }
                var newDoc = new Document
                {
                    Title = $"{dto.Title}",
                    FileUrl = s3ObjectKey,
                    SizeInBytes = dto.File.Length,
                    UploaderId = userId,
                    Status = dto.Status,
                    IsDeleted = 0,
                    CreatedAt = DateTime.UtcNow
                };
                if (!string.IsNullOrEmpty(dto.Tags))
                {
                    List<string> tagNames = dto.Tags.Split(',')
                        .Select(t => t.Trim().TrimStart('#'))
                        .Where(t => !string.IsNullOrEmpty(t))
                        .Distinct().ToList();
                    foreach (var tagName in tagNames)
                    {
                        string tagSlug = StringHelpers.GenerateSlug(tagName);
                        var existingTag = await _repo.tagsRepo.HasTag(tagSlug, tagName);

                        if (existingTag != null)
                        {
                            newDoc.Tags.Add(existingTag);
                        }
                        else
                        {
                            var newTag = new Tag
                            {
                                Name = tagName,
                                Slug = tagSlug
                            };
                            newDoc.Tags.Add(newTag);
                        }
                    }
                }
                bool iscreate = await _repo.documentsRepo.CreateAsync(newDoc);
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
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpGet("documents")]
        [Authorize]
        [EnableRateLimiting("read_limit")]
        public async Task<IActionResult> GetInforDoc([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            if (take > 50) { take = 50; }
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {
                List<ResDocumentDto> response = await _repo.documentsRepo.GetDocsByUserIdPagedAsync(userId, skip, take);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return BadRequest(new { message = "Có lỗi xảy ra khi tải dữ liệu." });
            }
        }
        [HttpGet("document/download/{docid}")]
        [EnableRateLimiting("read_limit")]
        public async Task<IActionResult> GetPdf(int docid)
        {
            Document? document = await _repo.documentsRepo.GetDocByIDAsync(docid);
            if (document == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin tài liệu trong database." });
            }
            bool fileExists = await _storageService.FileExistsAsync($"{document.FileUrl}", StorageType.Document);
            if (!fileExists)
            {
                return NotFound(new { message = "Tệp tin không tồn tại trên hệ thống lưu trữ đám mây." });
            }
            var s3Stream = await _storageService.GetFileStreamAsync(document.FileUrl, StorageType.Document);
            return File(s3Stream, "application/pdf", $"{document.Title}");
        }
        [HttpGet("document/detail/{docid}")]
        [EnableRateLimiting("read_limit")]
        public async Task<IActionResult> GetDetailDoc(int docid)
        {

            bool ishas = await _repo.documentsRepo.HasDocument(docid);
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            if (!ishas)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu." });
            }
            ResDocumentDto? result = await _repo.documentsRepo.GetDocWithUserByUserID(docid, userId);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu." });
            }
            return Ok(result);

        }
        [HttpPatch("document/movetotrash/{docid}")]
        [Authorize]
        [EnableRateLimiting("export_file_light")]
        public async Task<IActionResult> MoveToTrash(int docid, [FromBody] ReqMoveToTrashDTO isdelete)
        {
            bool ishas = await _repo.documentsRepo.HasDocument(docid);
            if (!ishas)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu." });
            }
            if (isdelete.isDeleted == false)
            {
                return BadRequest(new { message = "Yêu cầu không hợp lệ." });
            }
            bool isupdate = await _repo.documentsRepo.MoveToTrash(docid);
            if (isupdate)
            {
                return NoContent();
            }
            return BadRequest(new { message = "Lỗi khi cập nhật dữ liệu." });
        }
        [Authorize]
        [HttpPatch("document/{docid}")]
        [EnableRateLimiting("upload_limit")]
        public async Task<IActionResult> UpdateDocument(int docid, [FromForm] ReqUpdateDocumentDto dto)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            var document = await _repo.documentsRepo.GetDocByIDAsync(docid);
            if (document == null) return NotFound(new { message = "Tài liệu không tồn tại." });
            if (document.UploaderId != userId) return Forbid();

            bool isChanged = false;
            string oldTitle = document.Title;
            string oldFileUrl = document.FileUrl;
            if (!string.IsNullOrEmpty(dto.Title) && document.Title != dto.Title)
            {
                document.Title = dto.Title;
                isChanged = true;
            }

            if (!string.IsNullOrEmpty(dto.Description) && document.Description != dto.Description)
            {
                document.Description = dto.Description;
                isChanged = true;
            }

            if (!string.IsNullOrEmpty(dto.Status) && document.Status != dto.Status)
            {
                document.Status = dto.Status;
                isChanged = true;
            }
            if (dto.File != null && dto.File.Length > 0)
            {
                if (!string.IsNullOrEmpty(document.FileUrl))
                {
                    await _storageService.DeleteFileAsync(document.FileUrl, StorageType.Document);
                }
                string newKey = StringHelpers.Create_s3ObjectKey(dto.File.FileName, userId);
                using (var stream = dto.File.OpenReadStream())
                {
                    await _storageService.UploadFileAsync(stream, newKey, "application/pdf", StorageType.Document);
                }
                document.FileUrl = newKey;
                document.SizeInBytes = dto.File.Length;
                isChanged = true;
            }
            if (dto.Tags == null)
            {
                await _repo.tagsRepo.RemoveAllTagsByDocIdAsync(docid);
                isChanged = true;

            }
            else
            {
                await _repo.tagsRepo.RemoveAllTagsByDocIdAsync(docid);
                foreach (var tagName in dto.Tags)
                {
                    string tagSlug = StringHelpers.GenerateSlug(tagName);
                    var existingTag = await _repo.tagsRepo.HasTag(tagSlug, tagName);

                    if (existingTag != null)
                    {
                        document.Tags.Add(existingTag);
                    }
                    else
                    {
                        var newTag = new Tag
                        {
                            Name = tagName,
                            Slug = tagSlug
                        };
                        document.Tags.Add(newTag);
                    }
                }
                isChanged = true;
            }
            if (isChanged)
            {
                try
                {
                    document.UpdatedAt = DateTime.UtcNow;
                    var result = await _repo.documentsRepo.UpdateAsync(document);
                    if (!result) return BadRequest(new { message = "Lỗi khi cập nhật cơ sở dữ liệu." });
                    await _repo.SaveAllAsync();
                    return NoContent();
                }
                catch (Exception ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            return Ok(new { message = "Không có gì thay đổi." });
        }
        [Authorize]
        [HttpDelete("document/{docid}/fileUrl")]
        public async Task<IActionResult> DeleteDocumentFileUrl(int docid)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {

                bool result = await _repo.documentsRepo.DeleteFileUrl(docid);
                if (result)
                {
                    await _repo.SaveAllAsync();
                    return NoContent();
                }
                else
                {
                    return NotFound(new {message="Không timf thấy tài liệu"});
                }
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
