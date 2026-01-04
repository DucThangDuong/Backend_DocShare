using Application.DTOs;

namespace Application.IServices
{
    public interface IGoogleAuthService
    {
        Task<AuthResultDTO> HandleGoogleLoginAsync(string googleIdToken);
    }
}
