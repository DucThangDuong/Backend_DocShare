using API.Extensions;
using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class GetUploadedDocsEndpoint : EndpointWithoutRequest
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/me/documents/upload");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int currentId = HttpContext.User.GetUserId();
        if (currentId <= 0) { await Send.ResponseAsync(new { message = "ID người dùng không hợp lệ." }, 400, ct); return; }
        var result = await Repo.documentsRepo.GetDocumentUploadOfUser(currentId);
        await Send.ResponseAsync(result!, 200, ct);
    }
}
