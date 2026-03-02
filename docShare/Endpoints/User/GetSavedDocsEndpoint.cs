using API.Extensions;
using API.Features.User.Queries;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class GetSavedDocsEndpoint : EndpointWithoutRequest
{
    public GetSavedDocsHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/me/documents/save");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int currentId = HttpContext.User.GetUserId();
        if (currentId <= 0) { await Send.ResponseAsync(new { message = "ID người dùng không hợp lệ." }, 400, ct); return; }

        var result = await Handler.HandleAsync(new GetSavedDocsQuery(currentId), ct);
        await Send.ResponseAsync(result.Data!, 200, ct);
    }
}
