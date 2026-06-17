using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Auth;

public class VerifyForgotPasswordOtpRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(6, MinimumLength = 6)]
    public string Otp { get; set; } = null!;
}
