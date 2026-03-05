using Application.Common;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;
namespace API.Features.User.Commands
{
    public  record ReqUpdateResetPassCommand(string Email,string NewPassword,string ResetToken);
    public class UpdateResetPassHandler:ICommandHandler<ReqUpdateResetPassCommand>
    {
        public readonly IUsers _repo;
        public readonly IMemoryCache _cache;
        public UpdateResetPassHandler(IUsers repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }
        public async Task<Result> HandleAsync(ReqUpdateResetPassCommand cmd, CancellationToken ct = default)
        {
            Domain.Entities.User? user = await _repo.GetByEmailAsync(cmd.Email);
            if (user == null)
            {
                return Result.Failure("User not found", 404);
            }
            string cacheKey = $"ResetToken_{cmd.Email}";
            if (_cache.TryGetValue(cacheKey, out string? resetToken) && resetToken != null)
            {
                if (resetToken == cmd.ResetToken)
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(cmd.NewPassword);
                    await _repo.SaveChangeAsync();
                    _cache.Remove(cacheKey);
                    return Result.Success();
                }
                else
                {
                    return Result.Failure("Invalid reset token", 400);
                }

            }
            else
            {
                return Result.Failure("Reset token expired or not found", 400);
            }
        }
    }
}
