using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using API.Extensions;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using UglyToad.PdfPig;

namespace Application.Features.Documents.Commands;

public record CreateDocumentCommand(
    int UserId,
    string? Title,
    string? Status,
    string FileName,
    long FileLength,
    Stream FileStream,
    List<string>? Tags,
    int? UniversityId,
    int? UniversitySectionId
);

public class CreateDocumentHandler : ICommandHandler<CreateDocumentCommand>
{
    private readonly IUnitOfWork _repo;
    private readonly IStorageService _storageService;
    private readonly IRabbitMQService _rabbitMQ;
    private readonly IMemoryCache _cache;

    public CreateDocumentHandler(IUnitOfWork repo, IStorageService storageService, IRabbitMQService rabbitMQ, IMemoryCache cache)
    {
        _repo = repo;
        _storageService = storageService;
        _rabbitMQ = rabbitMQ;
        _cache = cache;
    }

    public async Task<Result> HandleAsync(CreateDocumentCommand cmd, CancellationToken ct = default)
    {
        string s3Key = StringHelpers.Create_s3ObjectKey_file(cmd.FileName, cmd.UserId);
        if (await _storageService.FileExistsAsync(s3Key, StorageType.Document))
            return Result.Failure($"File '{cmd.FileName}' Ä‘Ã£ tá»“n táº¡i trÃªn há»‡ thá»‘ng. Vui lÃ²ng Ä‘á»•i tÃªn hoáº·c kiá»ƒm tra láº¡i.");

        int pageCount;
        try { using var pdf = PdfDocument.Open(cmd.FileStream); pageCount = pdf.NumberOfPages; }
        catch { return Result.Failure("File PDF bá»‹ lá»—i hoáº·c bá»‹ há»ng, khÃ´ng thá»ƒ Ä‘á»c."); }

        cmd.FileStream.Position = 0;
        await _storageService.UploadFileAsync(cmd.FileStream, s3Key, "application/pdf", StorageType.Document);

        var newDoc = new Document
        {
            Title = $"{cmd.Title}", FileUrl = s3Key, SizeInBytes = cmd.FileLength,
            UploaderId = cmd.UserId, Status = cmd.Status, IsDeleted = 0,
            CreatedAt = DateTime.UtcNow, PageCount = pageCount,
        };

        if (cmd.UniversityId != null && cmd.UniversitySectionId != null)
        {
            if (!await _repo.UniversitiesRepo.SectionExistsAsync(cmd.UniversitySectionId.Value))
                return Result.Failure("Khoa/NgÃ nh khÃ´ng há»£p lá»‡.");
            newDoc.UniversitySectionId = cmd.UniversitySectionId.Value;
        }

        if (cmd.Tags != null && cmd.Tags.Any())
        {
            foreach (var tagName in cmd.Tags)
            {
                string slug = StringHelpers.GenerateSlug(tagName);
                var existing = await _repo.TagsRepo.GetBySlugAndNameAsync(slug, tagName);
                newDoc.Tags.Add(existing ?? new Tag { Name = tagName, Slug = slug });
            }
        }

        _repo.DocumentsRepo.Add(newDoc);
        await _repo.SaveChangesAsync();
        _cache.Remove($"user_stats_{cmd.UserId}");

        await _rabbitMQ.SendThumbnailRequest(new ThumbRequestEvent
        {
            DocId = newDoc.Id, FileUrl = newDoc.FileUrl, BucketName = "pdf-storage"
        });

        return Result.Success();
    }
}

