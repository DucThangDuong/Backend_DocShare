using API.Extensions;
using API.Features.UserActivity.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class VoteDocumentRequest
{
    public int DocId { get; set; }
    public bool? IsLike { get; set; }
}

public class VoteDocumentEndpoint : Endpoint<VoteDocumentRequest>
{
    public VoteDocumentHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/user-activity/vote/{docId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(VoteDocumentRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }

        try
        {
            var result = await Handler.HandleAsync(new VoteDocumentCommand(userId, req.DocId, req.IsLike), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.ResponseAsync(new { message = "Đã ghi nhận tương tác." }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
