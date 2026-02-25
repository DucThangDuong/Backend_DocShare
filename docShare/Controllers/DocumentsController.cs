using API.DTOs;
using API.Services;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using UglyToad.PdfPig;
using API.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace API.Controllers
{
    [Route("api/documents")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IUnitOfWork _repo;
        private readonly RabbitMQService _rabbitMQService;
        private readonly IStorageService _storageService;
        private readonly IMemoryCache _cache;

        public DocumentsController(IUnitOfWork repo, RabbitMQService rabbitMQService, IStorageService storageService, IMemoryCache cache)
        {
            _repo = repo;
            _rabbitMQService = rabbitMQService;
            _storageService = storageService;
            _cache = cache;
        }

        //get 
        [Authorize]
        [HttpGet]
        [EnableRateLimiting("read_standard")]
        public async Task<IActionResult> GetDocsOfUser([FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            if (take > 50) { take = 50; }
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
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
        
        [HttpGet("{docid}")]
        [EnableRateLimiting("read_public")]
        public async Task<IActionResult> GetDetailDoc(int docid)
        {
            int userId = User.GetUserId();
            string cacheKey = $"doc_detail_{docid}";
            if (_cache.TryGetValue(cacheKey, out ResDocumentDetailDto? cachedResult))
            {
                return Ok(cachedResult);
            }

            bool ishas = await _repo.documentsRepo.HasValue(docid);
            if (!ishas)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu." });
            }
            ResDocumentDetailDto? result = await _repo.documentsRepo.GetDocByUserIDAsync(docid, userId);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu." });
            }
            result.AvatarUrl = StringHelpers.GetFinalAvatarUrl(result.AvatarUrl ?? "");
            return Ok(result);
        }
        [HttpGet("{docid}/edit")]
        [Authorize]
        [EnableRateLimiting("read_standard")]
        public async Task<IActionResult> GetDetailEditDoc(int docid)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            bool ishas = await _repo.documentsRepo.HasValue(docid);
            if (!ishas)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu." });
            }
            ResDocumentDetailEditDto? result = await _repo.documentsRepo.GetDocumentDetailEditAsync(userId, docid);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy tài liệu." });
            }
            return Ok(result);
        }

        [Authorize]
        [HttpGet("stats")]
        [EnableRateLimiting("read_standard")]
        public async Task<IActionResult> GetUserStats()
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });

            string cacheKey = $"user_stats_{userId}";
            if (_cache.TryGetValue(cacheKey, out ResUserStatsDto? cachedStats))
            {
                return Ok(cachedStats);
            }

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

            var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(2));
            _cache.Set(cacheKey, userStatsDto, cacheOptions);

            return Ok(userStatsDto);
        }
        //post

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("write_heavy")]
        public async Task<IActionResult> PostDocument([FromForm] ReqCreateDocumentDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file." });

            if (Path.GetExtension(dto.File.FileName).ToLower() != ".pdf")
                return BadRequest(new { message = "Chỉ chấp nhận file PDF." });
            try
            {
                int userId = User.GetUserId();
                if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
                string s3ObjectKey = StringHelpers.Create_s3ObjectKey_file(dto.File.FileName, userId);
                bool isExist = await _storageService.FileExistsAsync(s3ObjectKey, StorageType.Document);
                if (isExist)
                {
                    return BadRequest($"File '{dto.File.FileName}' đã tồn tại trên hệ thống. Vui lòng đổi tên hoặc kiểm tra lại.");
                }

                int pageCount = 0;

                using (var stream = dto.File.OpenReadStream())
                {
                    try
                    {
                        using (var pdfDocument = PdfDocument.Open(stream))
                        {
                            pageCount = pdfDocument.NumberOfPages;
                        }
                    }
                    catch
                    {
                        return BadRequest(new { message = "File PDF bị lỗi hoặc bị hỏng, không thể đọc." });
                    }

                    stream.Position = 0;
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
                    CreatedAt = DateTime.UtcNow,
                    PageCount = pageCount,
                };
                if (dto.UniversityId != null && dto.UniversitySectionId!= null)
                {
                    var ishas = await _repo.universititesRepo.HasUniSection(dto.UniversitySectionId.Value);
                    if(!ishas)
                    {
                        return BadRequest(new { message = "Khoa/Ngành không hợp lệ." });
                    }
                    else
                    {
                        newDoc.UniversitySectionId = dto.UniversitySectionId.Value;
                    }
                }
                if (dto.Tags!=null && dto.Tags.Any())
                {
                    foreach (var tagName in dto.Tags)
                    {
                        string tagSlug = StringHelpers.GenerateSlug(tagName);
                        var existingTag = await _repo.tagsRepo.HasValue(tagSlug, tagName);

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
                _repo.documentsRepo.Create(newDoc);
                await _repo.SaveAllAsync();
                _cache.Remove($"user_stats_{userId}");

                var thumbMsg = new ThumbRequestEvent
                {
                    DocId = newDoc.Id,
                    FileUrl = newDoc.FileUrl,
                    BucketName = "pdf-storage"
                };
                await _rabbitMQService.SendThumbnailRequest(thumbMsg);
                return Created();

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [HttpPost("scan")]
        [Authorize]
        [EnableRateLimiting("write_heavy")]
        public async Task<IActionResult> PostCheckDocumentFile([FromForm] ReqCreateDocumentDTO dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn file." });
            if (Path.GetExtension(dto.File.FileName).ToLower() != ".pdf")
                return BadRequest(new { message = "Chỉ chấp nhận file PDF." });

            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {
                string s3ObjectKey = StringHelpers.Create_s3ObjectKey_file(dto.File.FileName, userId);
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
        //patch
        [HttpPatch("{docid}/trash")]
        [Authorize]
        [EnableRateLimiting("delete_action")]
        public async Task<IActionResult> PatchMoveToTrash(int docid, [FromBody] ReqMoveToTrashDTO isdelete)
        {
            int userId = User.GetUserId();
            bool ishas = await _repo.documentsRepo.HasValue(docid);
            try
            {

                if (!ishas)
                {
                    return NotFound(new { message = "Không tìm thấy tài liệu." });
                }
                if (isdelete.isDeleted == false)
                {
                    return BadRequest(new { message = "Yêu cầu không hợp lệ." });
                }
                await _repo.documentsRepo.MoveToTrash(docid);
                await _repo.SaveAllAsync();
                if (userId != 0)
                {
                    _cache.Remove($"doc_detail_{docid}_{userId}");
                    _cache.Remove($"user_stats_{userId}");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        [Authorize]
        [HttpPatch("{docid}")]
        [EnableRateLimiting("write_heavy")]
        public async Task<IActionResult> UpdateDocument(int docid, [FromForm] ReqUpdateDocumentDto dto)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {


                var document = await _repo.documentsRepo.GetDocByIDAsync(docid);
                if (document == null) return NotFound(new { message = "Tài liệu không tồn tại." });
                if (document.UploaderId != userId) return Forbid();

                string oldTitle = document.Title;
                string oldFileUrl = document.FileUrl;
                if (!string.IsNullOrEmpty(dto.Title) && document.Title != dto.Title)
                {
                    document.Title = dto.Title;
                }

                if (!string.IsNullOrEmpty(dto.Description) && document.Description != dto.Description)
                {
                    document.Description = dto.Description;
                }

                if (!string.IsNullOrEmpty(dto.Status) && document.Status != dto.Status)
                {
                    document.Status = dto.Status;
                }
                if (dto.File != null && dto.File.Length > 0)
                {
                    if (!string.IsNullOrEmpty(document.FileUrl))
                    {
                        await _storageService.DeleteFileAsync(document.FileUrl, StorageType.Document);
                    }
                    string s3ObjectKey = StringHelpers.Create_s3ObjectKey_file(dto.File.FileName, userId);
                    int pageCount = 0;
                    using (var stream = dto.File.OpenReadStream())
                    {
                        try
                        {
                            using (var pdfDocument = PdfDocument.Open(stream))
                            {
                                pageCount = pdfDocument.NumberOfPages;
                            }
                        }
                        catch
                        {
                            return BadRequest(new { message = "File PDF bị lỗi hoặc bị hỏng, không thể đọc." });
                        }

                        stream.Position = 0;
                        await _storageService.UploadFileAsync(stream, s3ObjectKey, "application/pdf", StorageType.Document);
                    }
                    document.FileUrl = s3ObjectKey;
                    document.SizeInBytes = dto.File.Length;
                    document.PageCount = pageCount;
                    document.CreatedAt = DateTime.UtcNow;
                    var thumbMsg = new ThumbRequestEvent
                    {
                        DocId = document.Id,
                        FileUrl = document.FileUrl,
                        BucketName = "pdf-storage"
                    };
                    await _rabbitMQService.SendThumbnailRequest(thumbMsg);
                }
                if (dto.UniversitySectionId != null)
                {
                    var ishas = await _repo.universititesRepo.HasUniSection( dto.UniversitySectionId.Value);
                    if (!ishas)
                    {
                        return BadRequest(new { message = "Khoa/Ngành không hợp lệ." });
                    }
                    else
                    {
                        document.UniversitySectionId = dto.UniversitySectionId.Value;
                    }
                }
                if (dto.UniversityId == null)
                {
                    document.UniversitySectionId = null;
                }
                if (dto.Tags == null)
                {
                    await _repo.tagsRepo.RemoveAllTagsOfDocIdAsync(docid);
                }
                else
                {
                    await _repo.tagsRepo.RemoveAllTagsOfDocIdAsync(docid);
                    foreach (var tagName in dto.Tags)
                    {
                        string tagSlug = StringHelpers.GenerateSlug(tagName);
                        var existingTag = await _repo.tagsRepo.HasValue(tagSlug, tagName);

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
                }
                document.UpdatedAt = DateTime.UtcNow;
                _repo.documentsRepo.Update(document);
                await _repo.SaveAllAsync();
                _cache.Remove($"doc_detail_{docid}_{userId}");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
        
        //delete
        [Authorize]
        [HttpDelete("{docid}/file")]
        [EnableRateLimiting("delete_action")]
        public async Task<IActionResult> DeleteDocumentFileUrl(int docid)
        {
            int userId = User.GetUserId();
            if (userId == 0) return Unauthorized(new { message = "Không xác định được danh tính người dùng." });
            try
            {

                bool result = await _repo.documentsRepo.HasValue(docid); 
                if (result)
                {
                    await _repo.documentsRepo.ClearFileContentUrl(docid);
                    await _repo.SaveAllAsync();
                    _cache.Remove($"doc_detail_{docid}_{userId}");
                    return NoContent();
                }
                else
                {
                    return NotFound(new {message="Không tìm thấy tài liệu"});
                }
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { message = $"Lỗi server: {ex.Message}" });
            }
        }
    }
}
