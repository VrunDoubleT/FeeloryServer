using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Auth;

public class VerifyRegisterOtpRequestDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "OTP is required")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be exactly 6 characters")]
    public string Otp { get; set; } = null!;
}
