using API.DTOs;
using Application.Interfaces;
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
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/universities/{universityId}/sections");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Options(x => x.RequireRateLimiting("write_standard"));
    }

    public override async Task HandleAsync(AddSectionRequest req, CancellationToken ct)
    {
        bool ishas = await Repo.universititesRepo.HasValue(req.UniversityId);
        if (!ishas) { await Send.ResponseAsync(null, 404, ct); return; }
        var result = await Repo.universititesRepo.AddSectionToUniversityAsync(req.UniversityId, req.Name);
        await Repo.SaveAllAsync();
        await Send.ResponseAsync(result, 200, ct);
    }
}
