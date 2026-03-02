using API.Extensions;
using API.Features.User.Queries;
using FastEndpoints;

namespace API.Endpoints.User;

public class GetUserDocumentsRequest
{
    public int UserId { get; set; }
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 10;
}

public class GetUserDocumentsEndpoint : Endpoint<GetUserDocumentsRequest>
{
    public GetUserDocumentsHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/{userId}/documents");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(GetUserDocumentsRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetUserDocumentsQuery(req.UserId, req.Skip, req.Take), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
