using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
