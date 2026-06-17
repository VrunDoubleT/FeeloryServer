using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "Username or Email is required")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;
}