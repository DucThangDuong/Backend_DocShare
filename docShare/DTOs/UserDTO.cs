using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReqUserUpdateDto
    {
        public string? Email { get; set; } = null!;
        public string? Password { get; set; } = null!;
        public string? FullName { get; set; }
    }
    public class ReqUpdateUserNameDto
    {
        public string? Username { get; set; }
    }
    public class ReqUpdatePasswordDto
    {
        public string OldPassword { get; set; } = null!;
        [Required]
        public string NewPassword { get; set; } = null!;
    }
    public class ReqSetPassword
    {

    }
}
