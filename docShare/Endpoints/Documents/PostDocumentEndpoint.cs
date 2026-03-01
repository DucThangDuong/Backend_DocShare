using API.DTOs;
using API.Extensions;
using API.Features.Documents.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Documents;

public class PostDocumentEndpoint : Endpoint<ReqCreateDocumentDTO>
{
    public CreateDocumentHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/documents");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        AllowFileUploads();
        Options(x => x.RequireRateLimiting("write_heavy"));
    }

    public override async Task HandleAsync(ReqCreateDocumentDTO req, CancellationToken ct)
    {
        if (req.File == null || req.File.Length == 0)
        { 
            await Send.ResponseAsync(new { message = "Vui lòng chọn file." }, 400, ct); 
            return; 
        }
        if (Path.GetExtension(req.File.FileName).ToLower() != ".pdf")
        { 
            await Send.ResponseAsync(new { message = "Chỉ chấp nhận file PDF." }, 400, ct);
            return; 
        }
        int userId = HttpContext.User.GetUserId();
        if (userId == 0)
        { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }
        using var stream = req.File.OpenReadStream();
        var command = new CreateDocumentCommand(
            UserId: userId, Title: req.Title, Status: req.Status,
            FileName: req.File.FileName, FileLength: req.File.Length, FileStream: stream,
            Tags: req.Tags, UniversityId: req.UniversityId, UniversitySectionId: req.UniversitySectionId);

        var result = await Handler.HandleAsync(command, ct);
        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(null, 201, ct);
    }
}
