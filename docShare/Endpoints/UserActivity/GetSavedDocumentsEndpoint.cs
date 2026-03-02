using API.Extensions;
using API.Features.UserActivity.Queries;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class GetSavedDocumentsEndpoint : EndpointWithoutRequest
{
    public GetSavedDocumentsHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user-activity/saved-library");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }

        var result = await Handler.HandleAsync(new GetSavedDocumentsQuery(userId), ct);
        await Send.ResponseAsync(result.Data, 200, ct);
    }
}
