using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Application.IServices;
using API.Extensions;
using API.Services;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.Documents.Commands;

public record ScanDocumentCommand(int UserId, string? Title, string FileName, long FileLength, Stream FileStream);

public class ScanDocumentHandler : ICommandHandler<ScanDocumentCommand>
{
    private readonly IStorageService _storageService;
    private readonly RabbitMQService _rabbitMQ;

    public ScanDocumentHandler(IStorageService storageService, RabbitMQService rabbitMQ)
    {
        _storageService = storageService;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<Result> HandleAsync(ScanDocumentCommand cmd, CancellationToken ct = default)
    {
        string s3Key = StringHelpers.Create_s3ObjectKey_file(cmd.FileName, cmd.UserId);
        if (await _storageService.FileExistsAsync(s3Key, StorageType.Document))
            return Result.Failure($"File '{cmd.FileName}' đã tồn tại trên hệ thống. Vui lòng đổi tên hoặc kiểm tra lại.");

        await _storageService.UploadFileAsync(cmd.FileStream, s3Key, "application/pdf", StorageType.Document);
        await _rabbitMQ.SendFileToScan(s3Key, $"{cmd.UserId}", $"{cmd.Title}");
        return Result.Success();
    }
}
