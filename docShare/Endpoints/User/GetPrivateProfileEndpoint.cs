using API.Extensions;
using API.Features.User.Queries;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.User;

public class GetPrivateProfileEndpoint : EndpointWithoutRequest
{
    public GetPrivateProfileHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/me/profile");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Token không hợp lệ hoặc thiếu thông tin định danh." }, 401, ct); return; }

        var result = await Handler.HandleAsync(new GetPrivateProfileQuery(userId), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
