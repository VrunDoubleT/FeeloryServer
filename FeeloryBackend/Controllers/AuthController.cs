using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FeeloryBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "OTP sent"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // POST api/auth/verify-otp
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyRegisterOtpRequestDto request)
    {
        var result = await _authService.VerifyRegisterOtpAsync(request);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "Verification successful and account created"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        if (!result.IsSuccess)
            return Unauthorized(new ApiErrorResponse(result.Error!));
        
        Response.Cookies.Append("refreshToken", result.Data!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,          
            SameSite = SameSiteMode.Lax,
        });

        return Ok(new ApiResponse<object>(
            new
            {
                accessToken = result.Data.AccessToken,
                refreshToken = result.Data.RefreshToken,      
            },
            "Login successful"
        ));
    }

    // POST api/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto? body)
    {
        var refreshToken = body?.RefreshToken
            ?? Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
            return BadRequest(new ApiErrorResponse("Refresh token is required"));

        var result = await _authService.RefreshTokenAsync(refreshToken);
        if (!result.IsSuccess)
            return Unauthorized(new ApiErrorResponse(result.Error!));
        
        Response.Cookies.Append("refreshToken", result.Data!.RefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,        
            SameSite = SameSiteMode.Lax,
        });

        return Ok(new ApiResponse<object>(
            new { accessToken = result.Data.AccessToken },
            "Token refreshed successfully"
        ));
    }

    // POST api/auth/change-password  [Requires JWT]
    [HttpPost("change-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        var userId = _currentUserService.GetUserId();
        var result = await _authService.ChangePasswordAsync(userId, request);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "Password changed successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // GET api/auth/me
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var userId = _currentUserService.GetUserId();
        var result = await _authService.GetCurrentUserAsync(userId);

        return result.IsSuccess
            ? Ok(new ApiResponse<object>(result.Data, "Profile retrieved successfully"))
            : NotFound(new ApiErrorResponse(result.Error!));
    }

    // POST api/auth/forgot-password/request-otp
    [HttpPost("forgot-password/request-otp")]
    public async Task<IActionResult> RequestForgotPasswordOtp([FromBody] ForgotPasswordRequestDto request)
    {
        var result = await _authService.RequestForgotPasswordOtpAsync(request);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "OTP sent successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // POST api/auth/forgot-password/verify-otp
    [HttpPost("forgot-password/verify-otp")]
    public async Task<IActionResult> VerifyForgotPasswordOtp([FromBody] VerifyForgotPasswordOtpRequestDto request)
    {
        var result = await _authService.VerifyForgotPasswordOtpAsync(request);
        return result.IsSuccess
            ? Ok(new ApiResponse<ForgotPasswordVerifyResponseDto>(result.Data!, "OTP verified successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }

    // POST api/auth/forgot-password/reset-password
    [HttpPost("forgot-password/reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        return result.IsSuccess
            ? Ok(new ApiResponse<object>(null, "Password reset successfully"))
            : BadRequest(new ApiErrorResponse(result.Error!));
    }
}