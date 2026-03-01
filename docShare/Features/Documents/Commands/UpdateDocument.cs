using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using API.Extensions;
using API.Services;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using UglyToad.PdfPig;

namespace API.Features.Documents.Commands;

public record UpdateDocumentCommand(
    int UserId, int DocId,
    string? Title, string? Description, string? Status,
    string? FileName, long? FileLength, Stream? FileStream,
    List<string>? Tags, int? UniversityId, int? UniversitySectionId
);

public class UpdateDocumentHandler : ICommandHandler<UpdateDocumentCommand>
{
    private readonly IUnitOfWork _repo;
    private readonly IStorageService _storageService;
    private readonly RabbitMQService _rabbitMQ;
    private readonly IMemoryCache _cache;

    public UpdateDocumentHandler(IUnitOfWork repo, IStorageService storageService, RabbitMQService rabbitMQ, IMemoryCache cache)
    {
        _repo = repo; _storageService = storageService; _rabbitMQ = rabbitMQ; _cache = cache;
    }

    public async Task<Result> HandleAsync(UpdateDocumentCommand cmd, CancellationToken ct = default)
    {
        var doc = await _repo.documentsRepo.GetDocByIDAsync(cmd.DocId);
        if (doc == null) return Result.Failure("Tài liệu không tồn tại.", 404);
        if (doc.UploaderId != cmd.UserId) return Result.Failure("Forbidden", 403);

        if (!string.IsNullOrEmpty(cmd.Title) && doc.Title != cmd.Title) doc.Title = cmd.Title;
        if (!string.IsNullOrEmpty(cmd.Description) && doc.Description != cmd.Description) doc.Description = cmd.Description;
        if (!string.IsNullOrEmpty(cmd.Status) && doc.Status != cmd.Status) doc.Status = cmd.Status;

        if (cmd.FileStream != null && cmd.FileLength > 0)
        {
            if (!string.IsNullOrEmpty(doc.FileUrl))
                await _storageService.DeleteFileAsync(doc.FileUrl, StorageType.Document);

            string s3Key = StringHelpers.Create_s3ObjectKey_file(cmd.FileName!, cmd.UserId);
            int pageCount;
            try { using var pdf = PdfDocument.Open(cmd.FileStream); pageCount = pdf.NumberOfPages; }
            catch { return Result.Failure("File PDF bị lỗi hoặc bị hỏng, không thể đọc."); }

            cmd.FileStream.Position = 0;
            await _storageService.UploadFileAsync(cmd.FileStream, s3Key, "application/pdf", StorageType.Document);
            doc.FileUrl = s3Key; doc.SizeInBytes = cmd.FileLength.Value;
            doc.PageCount = pageCount; doc.CreatedAt = DateTime.UtcNow;
            await _rabbitMQ.SendThumbnailRequest(new ThumbRequestEvent { DocId = doc.Id, FileUrl = doc.FileUrl, BucketName = "pdf-storage" });
        }

        if (cmd.UniversitySectionId != null)
        {
            if (!await _repo.universititesRepo.HasUniSection(cmd.UniversitySectionId.Value))
                return Result.Failure("Khoa/Ngành không hợp lệ.");
            doc.UniversitySectionId = cmd.UniversitySectionId.Value;
        }
        if (cmd.UniversityId == null) doc.UniversitySectionId = null;

        await _repo.tagsRepo.RemoveAllTagsOfDocIdAsync(cmd.DocId);
        if (cmd.Tags != null)
        {
            foreach (var tagName in cmd.Tags)
            {
                string slug = StringHelpers.GenerateSlug(tagName);
                var existing = await _repo.tagsRepo.HasValue(slug, tagName);
                doc.Tags.Add(existing ?? new Tag { Name = tagName, Slug = slug });
            }
        }

        doc.UpdatedAt = DateTime.UtcNow;
        _repo.documentsRepo.Update(doc);
        await _repo.SaveAllAsync();
        _cache.Remove($"doc_detail_{cmd.DocId}_{cmd.UserId}");
        return Result.Success();
    }
}
