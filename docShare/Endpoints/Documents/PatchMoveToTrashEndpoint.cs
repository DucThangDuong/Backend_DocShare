using API.Extensions;
using API.Features.Documents.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Documents;

public class PatchMoveToTrashRequest
{
    public int Docid { get; set; }
    public bool IsDeleted { get; set; }
}

public class PatchMoveToTrashEndpoint : Endpoint<PatchMoveToTrashRequest>
{
    public MoveToTrashHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Patch("/api/documents/{docid}/trash");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("delete_action"));
    }

    public override async Task HandleAsync(PatchMoveToTrashRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        var result = await Handler.HandleAsync(new MoveToTrashCommand(userId, req.Docid, req.IsDeleted), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.NoContentAsync(ct);
    }
}
