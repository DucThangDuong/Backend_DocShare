using API.DTOs;
using Application.Interfaces;
using FastEndpoints;

namespace API.Endpoints.Auth;

public class RegisterEndpoint : Endpoint<ReqRegisterDto>
{
    public IUsers Repo { get; set; } = null!;

    public override void Configure()
    {
        Post("/api/auth/register");
        AllowAnonymous();
        Options(x => x.RequireRateLimiting("auth_strict"));
    }

    public override async Task HandleAsync(ReqRegisterDto req, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Email) || string.IsNullOrEmpty(req.Password) || string.IsNullOrEmpty(req.Fullname))
        {
            await Send.ResponseAsync(new { message = "Vui lòng nhập đầy đủ thông tin." }, 400, ct);
            return;
        }
        try
        {
            bool isEmailExists = await Repo.HasEmailAsync(req.Email);
            if (isEmailExists)
            {
                await Send.ResponseAsync(new { message = "Email này đã tồn tại" }, 409, ct);
                return;
            }
            await Repo.CreateUserCustom(req.Email, req.Password, req.Fullname);
            await Repo.SaveChangeAsync();
            await Send.ResponseAsync(null, 201, ct);
        }
        catch (Exception ex)
        {
            await Send.ResponseAsync(new { message = $"Lỗi server: {ex.Message}" }, 500, ct);
        }
    }
}
