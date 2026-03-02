using API.Features.Universities.Queries;
using FastEndpoints;

namespace API.Endpoints.Universities;

public class GetSectionsRequest
{
    public int UniversityId { get; set; }
}

public class GetSectionsEndpoint : Endpoint<GetSectionsRequest>
{
    public GetSectionsHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/universities/{universityId}/sections");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_public"));
    }

    public override async Task HandleAsync(GetSectionsRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new GetSectionsQuery(req.UniversityId), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data!, 200, ct);
    }
}
