using API.DTOs;
using API.Features.User.Queries;
using Application.Common;
using Application.DTOs;
using FastEndpoints;

namespace API.Endpoints.Auth
{
    public class VerifyOTPEndpoint : Endpoint<ReqVerifyOTPDTO>
    {
        public GetVerifyOTPHandler Handler { get; set; } = null!;
        public override void Configure()
        {
            Post("api/auth/verify-otp");
            AllowAnonymous();
            Options(x => x.RequireRateLimiting("auth_strict"));
        }
        public override async Task HandleAsync(ReqVerifyOTPDTO req, CancellationToken ct)
        {
            Result<ResResetPassDto> result = await Handler.HandleAsync(new GetVerifyOTPQuery(req.Email, req.OTP.ToString()), ct);
            if (result.IsSuccess)
            {
                await Send.ResponseAsync(result.Data, result.StatusCode, ct);
            }
            else
            {
                await Send.ResponseAsync(result.Error, result.StatusCode, ct);
            }
        }
    }
}
