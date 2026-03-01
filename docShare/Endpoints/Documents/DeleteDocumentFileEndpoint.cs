using API.Extensions;
using API.Features.Documents.Commands;
using Application.Common;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Documents;

public class DeleteDocumentFileRequest
{
    public int Docid { get; set; }
}

public class DeleteDocumentFileEndpoint : Endpoint<DeleteDocumentFileRequest>
{
    public ClearDocumentFileHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Delete("/api/documents/{docid}/file");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("delete_action"));
    }

    public override async Task HandleAsync(DeleteDocumentFileRequest req, CancellationToken ct)
    {
        int userId = HttpContext.User.GetUserId();
        if (userId == 0)
        { 
            await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); 
            return; 
        }

        Result result = await Handler.HandleAsync(new ClearDocumentFileCommand(userId, req.Docid), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.NoContentAsync(ct);
    }
}
