using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Services.Interfaces;

public interface IAuthService
{
    // Register new user account (step 1: send OTP)
    Task<Result> RegisterAsync(RegisterRequestDto request);

    // Verify OTP and create real user (step 2)
    Task<Result> VerifyRegisterOtpAsync(VerifyRegisterOtpRequestDto request);

    // Authenticate user, return access token + refresh token
    Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request);

    // Refresh access token using a refresh token
    Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken);

    // Change password — revokes all existing refresh tokens (force logout other devices)
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request);

    // Get current logged-in user information
    Task<Result<UserDto>> GetCurrentUserAsync(Guid userId);

    // Forgot Password - Step 1: Request OTP
    Task<Result> RequestForgotPasswordOtpAsync(ForgotPasswordRequestDto request);

    // Forgot Password - Step 2: Verify OTP and get temporary token
    Task<Result<ForgotPasswordVerifyResponseDto>> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequestDto request);

    // Forgot Password - Step 3: Reset Password using temporary token
    Task<Result> ResetPasswordAsync(ResetPasswordRequestDto request);
}