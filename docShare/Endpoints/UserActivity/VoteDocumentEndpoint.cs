using API.Extensions;
using Application.Interfaces;
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
    public IUnitOfWork Repo { get; set; } = null!;

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
            await Repo.userActivityRepo.AddVoteDocumentAsync(userId, req.DocId, req.IsLike);
            await Repo.SaveAllAsync();
            await Send.ResponseAsync(new { message = "Đã ghi nhận tương tác." }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
