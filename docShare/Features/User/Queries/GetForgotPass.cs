using API.Services;
using Application.Common;
using Application.Interfaces;
using Application.DTOs;
using Microsoft.Extensions.Caching.Memory;
using API.DTOs;

namespace API.Features.User.Queries
{
    public record GetForgotPassQuery(string Email);
    public class GetForgotPassHandler : IQueryHandler<GetForgotPassQuery, bool>
    {
        private readonly IUsers _repo;
        private readonly RabbitMQService _rabbitMQ;
        private readonly IMemoryCache _cache;
        public GetForgotPassHandler(IUsers repo, RabbitMQService rabbitMQ, IMemoryCache cache)
        {
            _repo = repo;
            _rabbitMQ = rabbitMQ;
            _cache = cache;
        }
        public async Task<Result<bool>> HandleAsync(GetForgotPassQuery query, CancellationToken ct = default)
        {
            bool ishas = await _repo.HasEmailAsync(query.Email);
            if (ishas)
            {
                string OTP = Random.Shared.Next(100000, 999999).ToString();
                await _rabbitMQ.SendEmailResquest(new SendMailRequestDto
                {
                    Email = query.Email,
                    Subject = "Mã xác thực đặt lại mật khẩu của bạn",
                    HtmlMessage = $"Xin chào,\r\n\r\n" +
                    $"Chúng tôi đã nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.\r\n" +
                    $"Vui lòng sử dụng mã xác thực dưới đây để tiếp tục:\r\n\r\n" +
                    $"Mã OTP: {OTP}\r\n\r\n" +
                    $"Mã này có hiệu lực trong 5 phút và chỉ sử dụng một lần.\r\n\r\n" +
                    $"Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email.\r\n" +
                    $"Vì lý do bảo mật, không chia sẻ mã này với bất kỳ ai.\r\n\r\nTrân trọng."
                });
                string cacheKey = $"ForgotPass_{query.Email}";
                CacheOtpDTO otpDTO = new CacheOtpDTO
                {
                    Email = query.Email,
                    OTP = OTP,
                    Count = 0
                };
                _cache.Set(cacheKey, otpDTO, TimeSpan.FromMinutes(15));
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Email not found", 404);
        }
    }
}
