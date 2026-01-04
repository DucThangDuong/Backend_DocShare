using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải dài ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string Username { get; set; }
    }
    public class LoginDTo
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string? Password { get; set; }
    }
    public class GoogleLoginDTO
    {
        public string IdToken { get; set; } = string.Empty;
    }
}
