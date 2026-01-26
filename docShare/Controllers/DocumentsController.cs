using API.DTOs;
using API.Services;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
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
                return BadRequest("Vui lòng chọn file.");
            if (Path.GetExtension(dto.File.FileName).ToLower() != ".pdf")
                return BadRequest("Chỉ chấp nhận file PDF.");
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            try
            {
                string rawFileName = Path.GetFileNameWithoutExtension(dto.File.FileName);
                string safeFileName = SanitizeFileName(rawFileName);
                string extension = Path.GetExtension(dto.File.FileName).ToLower();
                string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                string s3ObjectKey = $"{userId}/{dateFolder}/{safeFileName}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
                bool isExist = await _storageService.FileExistsAsync(s3ObjectKey,StorageType.Document);
                if (isExist)
                {
                    return BadRequest($"File '{dto.File.FileName}' đã tồn tại trên hệ thống. Vui lòng đổi tên hoặc kiểm tra lại.");
                }
                using (var stream = dto.File.OpenReadStream())
                {
                    await _storageService.UploadFileAsync(stream, s3ObjectKey, "application/pdf",StorageType.Document);
                }
                await _rabbitMQService.SendFileToScan(s3ObjectKey, $"{userId}", $"{dto.Title}");
                return Ok(new
                {
                    message = "File đã được tải lên và đang chờ quét.",
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
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
                string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                string rawFileName = Path.GetFileNameWithoutExtension(dto.File.FileName);
                string safeFileName = SanitizeFileName(rawFileName);
                string extension = Path.GetExtension(dto.File.FileName).ToLower();
                string dateFolder = DateTime.Now.ToString("yyyy-MM-dd");
                string s3ObjectKey = $"{userId}/{dateFolder}/{safeFileName}_{Guid.NewGuid().ToString().Substring(0, 8)}{extension}";
                bool isExist = await _storageService.FileExistsAsync(s3ObjectKey,StorageType.Document);
                if (isExist)
                {
                    return BadRequest($"File '{dto.File.FileName}' đã tồn tại trên hệ thống. Vui lòng đổi tên hoặc kiểm tra lại.");
                }
                using (var stream = dto.File.OpenReadStream())
                {
                    await _storageService.UploadFileAsync(stream, s3ObjectKey, "application/pdf", StorageType.Document);
                }
                if (userId == null)
                {
                    return Unauthorized();
                }
                var newDoc = new Document
                {
                    Title = $"{dto.Title}",
                    FileUrl = s3ObjectKey,
                    SizeInBytes = dto.File.Length,
                    UploaderId = int.Parse(userId!),
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
                        string tagSlug = GenerateSlug(tagName);
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
                Console.Error.WriteLine(ex);
                return BadRequest("Có lỗi xảy ra khi tải dữ liệu.");
            }
        }
        [HttpGet("document/download/{docid}")]
        public async Task<IActionResult> GetPdf(int docid)
        {
            Document? document = await _repo.documentsRepo.GetDocByIDAsync(docid);
            if (document == null)
            {
                return NotFound("Không tìm thấy thông tin tài liệu trong database.");
            }
            bool fileExists = await _storageService.FileExistsAsync($"{document.FileUrl}",StorageType.Document);
            if (!fileExists)
            {
                return NotFound("Tệp tin không tồn tại trên hệ thống lưu trữ đám mây.");
            }
            var s3Stream = await _storageService.GetFileStreamAsync(document.FileUrl, StorageType.Document);
            return File(s3Stream, "application/pdf", $"{document.Title}");
        }
        [HttpGet("document/detail/{docid}")]
        public async Task<IActionResult> GetDetailDoc(int docid) {
            bool ishas = await _repo.documentsRepo.HasDocument(docid);
            if (!ishas)
            {
                return NotFound();
            }
            ResDocumentDto? result = await _repo.documentsRepo.GetDocWithUserByUserID(docid);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);

        }
        [HttpPatch("document/{docid}")]
        public async Task<IActionResult> MoveToTrash(int docid, [FromBody] ReqMoveToTrashDTO isdelete)
        {
            bool ishas = await _repo.documentsRepo.HasDocument(docid);
            if (!ishas)
            {
                return NotFound();
            }
            if(isdelete.isDeleted == false)
            {
                return BadRequest("Yêu cầu không hợp lệ.");
            }
            bool isupdate = await _repo.documentsRepo.MoveToTrash(docid);
            if (isupdate)
            {
                return NoContent();
            }
            return BadRequest("Lỗi khi cập nhật dữ liệu.");
        }
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;
            const string KeyChars = "áàạảãâấầậẩẫăắằặẳẵÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴéèẹẻẽêếềệểễÉÈẸẺẼÊẾỀỆỂỄóòọỏõôốồộổỗơớờợởỡÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠúùụủũưứừựửữÚÙỤỦŨƯỨỪỰỬỮíìịỉĩÍÌỊỈĨđĐýỳỵỷỹÝỲỴỶỸ ";
            const string ReplChars = "aaaaaaaaaaaaaaaaaAAAAAAAAAAAAAAAAAeeeeeeeeeeeEEEEEEEEEEEoooooooooooooooooOOOOOOOOOOOOOOOOOuuuuuuuuuuuUUUUUUUUUUUiiiiiIIIIIdDyyyyyYYYYY_";
            StringBuilder sb = new StringBuilder(fileName.Length);
            foreach (char c in fileName)
            {
                if (c == '^') continue;

                int index = KeyChars.IndexOf(c);
                if (index != -1)
                {
                    sb.Append(ReplChars[index]);
                }
                else
                {
                    sb.Append(c);
                }
            }
            string result = sb.ToString();
            return Regex.Replace(result, @"[^a-zA-Z0-9_\-\.]", "");
        }
        public static string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = ConvertToUnSign(str);
            str = Regex.Replace(str, @"\s+", "-");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }
        private static string ConvertToUnSign(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

    }
}
