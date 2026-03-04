using API.DTOs;
using API.Features.User.Queries;
using Application.Common;
using FastEndpoints;

namespace API.Endpoints.Auth
{
    public class SendMailForgotPassEndpoint :Endpoint<ReqForgotPasswordDTO>
    {
        public GetForgotPassHandler Handler { get; set; } = null!;
        public override void Configure()
        {
            Post("api/auth/forgot-password");
            Options(x => x.RequireRateLimiting("auth_strict"));
            AllowAnonymous();
        }
        public override async Task HandleAsync(ReqForgotPasswordDTO req, CancellationToken ct)
        {
            Result result = await Handler.HandleAsync(new GetForgotPassQuery(req.Email), ct);
            if (!result.IsSuccess)
            {
                await Send.ResponseAsync(result.Error, result.StatusCode, ct);
            }
            else
            {
                await Send.ResponseAsync("OTP has been sent to your email", result.StatusCode, ct);
            }
        }
    }
}
