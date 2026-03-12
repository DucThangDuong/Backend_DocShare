using API.DTOs;
using API.Extensions;
using Application.Features.Documents.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Documents;


public class UpdateDocumentEndpoint : Endpoint<ReqUpdateDocumentDto>
{
    public UpdateDocumentHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/documents/{docid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Options(x => x.RequireRateLimiting("write_heavy"));
    }

    public override async Task HandleAsync(ReqUpdateDocumentDto req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0)
        { await Send.ResponseAsync(new { message = "KhÃ´ng xÃ¡c Ä‘á»‹nh Ä‘Æ°á»£c danh tÃ­nh ngÆ°á»i dÃ¹ng." }, 401, ct); return; }

        Stream? fileStream = req.File?.OpenReadStream();
        var command = new UpdateDocumentCommand(
            UserId: userId, DocId: req.Docid,
            Title: req.Title, Description: req.Description, Status: req.Status,
            FileName: req.File?.FileName, FileLength: req.File?.Length, FileStream: fileStream,
            Tags: req.Tags, UniversityId: req.UniversityId, UniversitySectionId: req.UniversitySectionId);

        var result = await Handler.HandleAsync(command, ct);
        fileStream?.Dispose();

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.NoContentAsync(ct);
    }
}
