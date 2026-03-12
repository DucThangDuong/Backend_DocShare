using API.Extensions;
using Application.Features.UserActivity.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.UserActivity;

public class SaveDocumentRequest
{
    public int DocId { get; set; }
}

public class SaveDocumentEndpoint : Endpoint<SaveDocumentRequest>
{
    public SaveDocumentHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/user-activity/save/{docId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(SaveDocumentRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0) { await Send.ResponseAsync(new { message = "KhÃ´ng xÃ¡c Ä‘á»‹nh Ä‘Æ°á»£c danh tÃ­nh ngÆ°á»i dÃ¹ng." }, 401, ct); return; }

        try
        {
            var result = await Handler.HandleAsync(new SaveDocumentCommand(userId, req.DocId), ct);

            if (!result.IsSuccess)
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            else
                await Send.ResponseAsync(new { message = "LÆ°u tÃ i liá»‡u thÃ nh cÃ´ng" }, 200, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = ex.Message }, 400, ct);
        }
    }
}
