using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.IServices
{
    public interface IJwtTokenService
    {
        public string GenerateAccessToken(int userId, string email, string role);
        public RefreshToken GenerateRefreshToken();
    }
}
