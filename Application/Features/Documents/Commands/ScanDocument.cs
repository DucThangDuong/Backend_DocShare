using Application.Common;
using Application.DTOs;
using Application.IServices;
using API.Extensions;

namespace Application.Features.Documents.Commands;

public record ScanDocumentCommand(int UserId, string? Title, string FileName, long FileLength, Stream FileStream);

public class ScanDocumentHandler : ICommandHandler<ScanDocumentCommand>
{
    private readonly IStorageService _storageService;
    private readonly IRabbitMQService _rabbitMQ;

    public ScanDocumentHandler(IStorageService storageService, IRabbitMQService rabbitMQ)
    {
        _storageService = storageService;
        _rabbitMQ = rabbitMQ;
    }

    public async Task<Result> HandleAsync(ScanDocumentCommand cmd, CancellationToken ct = default)
    {
        string s3Key = StringHelpers.Create_s3ObjectKey_file(cmd.FileName, cmd.UserId);
        if (await _storageService.FileExistsAsync(s3Key, StorageType.Document))
            return Result.Failure($"File '{cmd.FileName}' Ä‘Ã£ tá»“n táº¡i trÃªn há»‡ thá»‘ng. Vui lÃ²ng Ä‘á»•i tÃªn hoáº·c kiá»ƒm tra láº¡i.");

        await _storageService.UploadFileAsync(cmd.FileStream, s3Key, "application/pdf", StorageType.Document);
        await _rabbitMQ.SendFileToScan(s3Key, $"{cmd.UserId}", $"{cmd.Title}");
        return Result.Success();
    }
}
