using API.Extensions;
using Application.Interfaces;
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
    public IUnitOfWork Repo { get; set; } = null!;

    public override void Configure()
    {
        Get("/api/user/{userId}/documents");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("read_standard"));
    }

    public override async Task HandleAsync(GetUserDocumentsRequest req, CancellationToken ct)
    {
        if (req.Take > 50) req.Take = 50;
        bool ishas = await Repo.usersRepo.HasValue(req.UserId);
        if (!ishas) { await Send.ResponseAsync(new { message = "Không xác định được danh tính người dùng." }, 401, ct); return; }
        try
        {
            var response = await Repo.documentsRepo.GetDocsByUserIdPagedAsync(req.UserId, req.Skip, req.Take);
            await Send.ResponseAsync(response, 200, ct);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            await Send.ResponseAsync(new { message = "Có lỗi xảy ra khi tải dữ liệu." }, 400, ct);
        }
    }
}
