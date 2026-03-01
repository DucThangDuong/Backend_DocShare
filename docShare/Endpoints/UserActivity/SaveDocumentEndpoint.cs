using API.Extensions;
using Application.Interfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class SaveDocumentRequest
{
    public int DocId { get; set; }
}

public class SaveDocumentEndpoint : Endpoint<SaveDocumentRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/user-activity/save/{docId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(SaveDocumentRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }
        try
        {
            await Repo.userActivityRepo.AddUserSaveDocumentAsync(userId, req.DocId);
            await Repo.SaveAllAsync();
            await Send.ResponseAsync(new { message = "Lưu tài liệu thành công" }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
