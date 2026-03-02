using API.Features.Universities.Commands;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace API.Endpoints.Universities;

public class AddSectionRequest
{
    public int UniversityId { get; set; }
    public string Name { get; set; } = null!;
}

public class AddSectionEndpoint : Endpoint<AddSectionRequest>
{
    public API.Features.Universities.Commands.AddSectionHandler Handler { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/universities/{universityId}/sections");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(AddSectionRequest req, CancellationToken ct)
    {
        var result = await Handler.HandleAsync(new AddSectionCommand(req.UniversityId, req.Name), ct);

        if (!result.IsSuccess)
            await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
        else
            await Send.ResponseAsync(result.Data, 200, ct);
    }
}
