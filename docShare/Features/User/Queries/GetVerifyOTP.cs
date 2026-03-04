using API.DTOs;
using Application.Common;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace API.Features.User.Queries
{
    public record GetVerifyOTPQuery(string Email, string OTP);
    public class CacheOtpDTO
    {
        public string Email { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public int Count { get; set; } = 0;
    }
    public class GetVerifyOTPHandler : IQueryHandler<GetVerifyOTPQuery, ResResetPassDto>
    {
        private readonly IUsers _repo;
        private readonly IMemoryCache _cache;
        public GetVerifyOTPHandler(IUsers repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        public async Task<Result<ResResetPassDto>> HandleAsync(GetVerifyOTPQuery query, CancellationToken ct = default)
        {
            string cacheKey = $"ForgotPass_{query.Email}";
            bool ishas = await _repo.HasEmailAsync(query.Email);
            if (!ishas) return Result<ResResetPassDto>.Failure("Email not found", 404);
            if (_cache.TryGetValue(cacheKey, out CacheOtpDTO? cachedOTP) && cachedOTP != null)
            {
                if (cachedOTP.OTP == query.OTP)
                {
                    _cache.Remove(cacheKey);

                    string resetToken = Guid.NewGuid().ToString();
                    _cache.Set($"ResetToken_{query.Email}", resetToken, TimeSpan.FromMinutes(5));
                    ResResetPassDto res = new ResResetPassDto { ResetToken = resetToken };
                    return Result<ResResetPassDto>.Success(res);
                }

                cachedOTP.Count++;
                _cache.Set(cacheKey, cachedOTP, TimeSpan.FromMinutes(15));

                if (cachedOTP.Count >= 5 || cachedOTP.Email != query.Email)
                {
                    _cache.Remove(cacheKey);
                    return Result<ResResetPassDto>.Failure("OTP expired due to too many attempts", 400);
                }

                return Result<ResResetPassDto>.Failure("Invalid OTP", 400);
            }
            else
            {
                return Result<ResResetPassDto>.Failure("OTP expired or not found", 404);
            }
        }
    }
}
