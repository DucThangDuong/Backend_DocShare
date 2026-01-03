using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;

namespace Application.IServices
{
    public interface IJwtTokenService
    {
        public string GenerateToken(ClaimsPrincipal principal);
        public string GenerateAccessToken(int userId, string email, string role);
        public RefreshToken GenerateRefreshToken();
    }
}
