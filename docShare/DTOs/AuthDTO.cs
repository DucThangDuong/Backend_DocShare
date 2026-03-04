using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ReqRegisterDto
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải dài ít nhất 6 ký tự")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        public string Fullname { get; set; }
    }
    public class ReqLoginDTo
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string? Password { get; set; }
    }
    public class ReqGoogleLoginDTO
    {
        public string IdToken { get; set; } = string.Empty;
    }
    public class ReqForgotPasswordDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;
    }
    public class ReqVerifyOTPDTO
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = null!;
        [Required(ErrorMessage = "OTP không được để trống")]
        [MaxLength(6, ErrorMessage = "OTP phải có đúng 6 ký tự"), MinLength(6, ErrorMessage = "OTP phải có đúng 6 ký tự")]
        public string OTP { get; set; } = null!;
    }
    public class CacheOtpDTO
    {
        public string Email { get; set; } = null!;
        public string OTP { get; set; } = null!;
        public int Count { get; set; }
    }
    public class ReqResetPassDto
    {
        public string Email { get; set; } = null!;
        public string ResetToken { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
