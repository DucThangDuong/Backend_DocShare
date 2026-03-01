using API.Extensions;
using API.Features.Documents.Queries;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;

namespace API.Endpoints.Documents;

public class GetDetailEditDocRequest
{
    public int Docid { get; set; }
}

public class GetDetailEditDocEndpoint : Endpoint<GetDetailEditDocRequest>
{
    public GetDocumentEditDetailHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/documents/{docid}/edit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("read_standard"));
    }
    public override async Task HandleAsync(GetDetailEditDocRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0)
        { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }

        var result = await Handler.HandleAsync(new GetDocumentEditDetailQuery(userId, req.Docid), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
