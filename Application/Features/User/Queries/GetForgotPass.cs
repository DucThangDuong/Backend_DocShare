using Application.Common;
using Application.Interfaces;
using Application.IServices;
using Application.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace Application.Features.User.Queries
{
    public record GetForgotPassQuery(string Email);
    public class GetForgotPassHandler : IQueryHandler<GetForgotPassQuery, bool>
    {
        private readonly IUsers _repo;
        private readonly IRabbitMQService _rabbitMQ;
        private readonly IMemoryCache _cache;
        public GetForgotPassHandler(IUsers repo, IRabbitMQService rabbitMQ, IMemoryCache cache)
        {
            _repo = repo;
            _rabbitMQ = rabbitMQ;
            _cache = cache;
        }
        public async Task<Result<bool>> HandleAsync(GetForgotPassQuery query, CancellationToken ct = default)
        {
            bool ishas = await _repo.EmailExistsAsync(query.Email);
            if (ishas)
            {
                string OTP = Random.Shared.Next(100000, 999999).ToString();
                await _rabbitMQ.SendEmailResquest(new SendMailRequestDto
                {
                    Email = query.Email,
                    Subject = "Mã xác th?c d?t l?i m?t kh?u c?a b?n",
                    HtmlMessage = $"Xin chào,\r\n\r\n" +
                    $"Chúng tôi dã nh?n du?c yêu c?u d?t l?i m?t kh?u cho tài kho?n c?a b?n.\r\n" +
                    $"Vui lòng s? d?ng mã xác th?c du?i dây d? ti?p t?c:\r\n\r\n" +
                    $"Mã OTP: {OTP}\r\n\r\n" +
                    $"Mã này có hi?u l?c trong 5 phút và ch? s? d?ng m?t l?n.\r\n\r\n" +
                    $"N?u b?n không th?c hi?n yêu c?u này, vui lòng b? qua email.\r\n" +
                    $"Vì lý do b?o m?t, không chia s? mã này v?i b?t k? ai.\r\n\r\nTrân tr?ng."
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

