using API.DTOs;
using API.Extensions;
using Application.Features.Documents.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Documents;

public class PostScanDocumentEndpoint : Endpoint<ReqCreateDocumentDTO>
{
    public ScanDocumentHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/documents/scan");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Options(x => x.RequireRateLimiting("write_heavy"));
    }

    public override async Task HandleAsync(ReqCreateDocumentDTO req, CancellationToken ct)
    {
        if (req.File == null || req.File.Length == 0)
        { await Send.ResponseAsync(new { message = "Vui lÃ²ng chá»n file." }, 400, ct); return; }
        if (Path.GetExtension(req.File.FileName).ToLower() != ".pdf")
        { await Send.ResponseAsync(new { message = "Chá»‰ cháº¥p nháº­n file PDF." }, 400, ct); return; }

        int userId = HttpContext.User.GetUserId();
        if (userId == 0)
        { await Send.ResponseAsync(new { message = "KhÃ´ng xÃ¡c Ä‘á»‹nh Ä‘Æ°á»£c danh tÃ­nh ngÆ°á»i dÃ¹ng." }, 401, ct); return; }

        using var stream = req.File.OpenReadStream();
        var command = new ScanDocumentCommand(userId, req.Title, req.File.FileName, req.File.Length, stream);

        var result = await Handler.HandleAsync(command, ct);
        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(new { message = "File Ä‘Ã£ Ä‘Æ°á»£c táº£i lÃªn vÃ  Ä‘ang chá» quÃ©t." }, 200, ct);
    }
}
