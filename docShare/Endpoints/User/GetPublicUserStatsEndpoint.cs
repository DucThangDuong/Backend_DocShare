using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;

namespace API.Endpoints.User;

public class GetPublicUserStatsRequest
{
    public int UserId { get; set; }
}

public class GetPublicUserStatsEndpoint : Endpoint<GetPublicUserStatsRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/{userId}/stats");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(GetPublicUserStatsRequest req, CancellationToken ct)
    {
        bool ishas = await Repo.usersRepo.HasValue(req.UserId);
        if (!ishas) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }
        var userStatsDto = await Repo.documentsRepo.GetUserStatsAsync(req.UserId);
        if (userStatsDto == null)
        {
            userStatsDto = new ResUserStatsDto { SavedCount = 0, UploadCount = 0, TotalLikesReceived = 0 };
        }
        await Send.ResponseAsync(userStatsDto, 200, ct);
    }
}
