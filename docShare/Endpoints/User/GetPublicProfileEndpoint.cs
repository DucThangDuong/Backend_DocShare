using API.Extensions;
using Application.DTOs;
using Application.Interfaces;
using FastEndpoints;

namespace API.Endpoints.User;

public class GetPublicProfileRequest
{
    public int UserId { get; set; }
}

public class GetPublicProfileEndpoint : Endpoint<GetPublicProfileRequest>
{
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/{userId}/profile");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(GetPublicProfileRequest req, CancellationToken ct)
    {
        int currentId = HttpContext.User.GetUserId();
        if (req.UserId <= 0) { await Send.ResponseAsync(new { message = "ID người dùng không hợp lệ." }, 400, ct); return; }
        var result = await Repo.usersRepo.GetUserPublicProfileAsync(req.UserId, currentId);
        if (result == null) { await Send.ResponseAsync(new { message = "Không tìm thấy thông tin người dùng." }, 404, ct); return; }
        result.avatarUrl = StringHelpers.GetFinalAvatarUrl(result.avatarUrl ?? "");
        await Send.ResponseAsync(result, 200, ct);
    }
}
