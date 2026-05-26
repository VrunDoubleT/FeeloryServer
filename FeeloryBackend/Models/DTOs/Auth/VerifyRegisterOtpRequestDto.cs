namespace FeeloryBackend.Models.DTOs.Auth;

public class VerifyRegisterOtpRequestDto
{
    public string Email { get; set; } = null!;
    public string Otp { get; set; } = null!;
}
