using API.DTOs;
using API.Features.User.Commands;
using FastEndpoints;

namespace API.Endpoints.Auth
{
    public class ResetPassEndpoint : Endpoint<ReqResetPassDto>
    {
        public UpdateResetPassHandler _handler { get; set; } = null!;
        public override void Configure()
        {
            Post("/api/auth/reset-password");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("auth_strict"));
        }
        public async override Task HandleAsync(ReqResetPassDto req, CancellationToken ct)
        {
            var result = await _handler.HandleAsync(new ReqUpdateResetPassCommand(req.Email, req.NewPassword, req.ResetToken), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(new { message = "Update success" }, 200, ct);
            }
            else
            {
                await Send.ResponseAsync(new { message = result.Error }, result.StatusCode, ct);
            }
        }
    }
}
