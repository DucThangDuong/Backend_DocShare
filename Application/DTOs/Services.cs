using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AuthResultDTO
    {
        public bool IsSuccess { get; set; }
        public string? CustomJwtToken { get; set; }
        public string? ErrorMessage { get; set; }
    }
    public class RefreshToken
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
    }
    public class ScanFileResultDto
    {
        public string FilePath { get; set; }
        public string Status { get; set; }
        public string UserId { get; set; }
        public string DocIdDto { get; set; }
        public string Message { get; set; }
    }
}
