using API.DTOs;
using API.Extensions;
using API.Features.Documents.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Documents;

public class UpdateDocumentRequest
{
    public int Docid { get; set; }
    public IFormFile? File { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
    public string? Status { get; set; }
    public int? UniversityId { get; set; }
    public int? UniversitySectionId { get; set; }
}

public class UpdateDocumentEndpoint : Endpoint<UpdateDocumentRequest>
{
    public UpdateDocumentHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/documents/{docid}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Options(x => x.RequireRateLimiting("write_heavy"));
    }

    public override async Task HandleAsync(UpdateDocumentRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0)
        { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }

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
