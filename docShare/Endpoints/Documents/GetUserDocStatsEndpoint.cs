using API.Extensions;
using API.Features.Documents.Queries;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Documents;

public class GetUserDocStatsEndpoint : EndpointWithoutRequest
{
    public GetUserDocStatsHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/documents/stats");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0)
        { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }

        var result = await Handler.HandleAsync(new GetUserDocStatsQuery(userId), ct);
        await Send.ResponseAsync(result.Data, 200, ct);
    }
}
