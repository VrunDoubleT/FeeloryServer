using FeeloryBackend.Commons;
using FeeloryBackend.Data;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using System.Security.Cryptography;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Users;
using FeeloryBackend.Utils;

namespace FeeloryBackend.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasherService _passwordHasherService;
    private readonly UserCreatedPublisher _userCreatedPublisher;
    private readonly EmailPublisher _emailPublisher;
    private readonly IDistributedCache _cache;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly GenerateUniqueUserName _generateUniqueUserName;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AuthService(
        AppDbContext dbContext,
        IJwtTokenService jwtTokenService,
        EmailPublisher emailPublisher,
        IPasswordHasherService passwordHasherService,
        UserCreatedPublisher userCreatedPublisher,
        IDistributedCache cache,
        IRefreshTokenService refreshTokenService,
        GenerateUniqueUserName generateUniqueUserName)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
        _passwordHasherService = passwordHasherService;
        _userCreatedPublisher = userCreatedPublisher;
        _emailPublisher = emailPublisher;
        _cache = cache;
        _refreshTokenService = refreshTokenService;
        _generateUniqueUserName = generateUniqueUserName;
    }

    // ─── Step 1: Send OTP ────────────────────────────────────────────────────

    public async Task<Result> RegisterAsync(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(r => r.Email.ToLower() == normalizedEmail);
        if (user != null)
            return Result.Fail("User with the same email already exists");

        var passwordHash = _passwordHasherService.HashPassword(request.Password);

        var tempData = new TempRegisterData
        {
            DisplayName = request.DisplayName,
            Email = request.Email,
            PasswordHash = passwordHash
        };

        var serializedData = JsonSerializer.Serialize(tempData, _jsonOptions);
        await _cache.SetStringAsync(
            $"register:data:{normalizedEmail}",
            serializedData,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) }
        );

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        await _cache.SetStringAsync(
            $"register:otp:{normalizedEmail}",
            otp,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }
        );

        await _emailPublisher.SendEmailAsync(new EmailMessage
        {
            To = request.Email,
            Subject = "Feelory Verification Code",
            Body = $"Your OTP verification code is <strong>{otp}</strong>. It is valid for 5 minutes."
        });

        return Result.Ok();
    }

    // ─── Step 2: Verify OTP & Create User ────────────────────────────────────

    public async Task<Result> VerifyRegisterOtpAsync(VerifyRegisterOtpRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var cachedOtp = await _cache.GetStringAsync($"register:otp:{normalizedEmail}");
        if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != request.Otp)
            return Result.Fail("Invalid or expired OTP");

        var cachedDataJson = await _cache.GetStringAsync($"register:data:{normalizedEmail}");
        if (string.IsNullOrEmpty(cachedDataJson))
            return Result.Fail("Registration data has expired. Please register again.");

        var tempData = JsonSerializer.Deserialize<TempRegisterData>(cachedDataJson, _jsonOptions);
        if (tempData == null)
            return Result.Fail("Failed to deserialize registration data.");

        var userExists = await _dbContext.Users.AnyAsync(r => r.Email.ToLower() == normalizedEmail);
        if (userExists)
            return Result.Fail("User with the same email already exists");

        var username = await _generateUniqueUserName.GenerateUniqueUsernameAsync(tempData.DisplayName);

        var newUser = new User
        {
            Email = tempData.Email,
            DisplayName = tempData.DisplayName,
            PasswordHash = tempData.PasswordHash,
            CreatedAt = DateTime.UtcNow,
            Username = username
        };

        await _dbContext.Users.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();

        await _userCreatedPublisher.PublishUserCreatedAsync(new UserCreatedMessage()
        {
            UserId = newUser.Id
        });

        await _cache.RemoveAsync($"register:otp:{normalizedEmail}");
        await _cache.RemoveAsync($"register:data:{normalizedEmail}");

        return Result.Ok();
    }

    // ─── Login ───────────────────────────────────────────────────────────────

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(r => r.Email == request.Email
                                   || r.Username == request.Email);
        if (user == null)
            return Result<AuthResponseDto>.Fail("User not found");

        if (!_passwordHasherService.VerifyPassword(request.Password, user.PasswordHash))
            return Result<AuthResponseDto>.Fail("Invalid password");

        var accessToken = _jwtTokenService.GenerateToken(user);

        var refreshTokenData = _refreshTokenService.GenerateRefreshToken(user.Id);
        await _refreshTokenService.SaveRefreshTokenAsync(
            refreshTokenData.RefreshToken,
            user.Id,
            refreshTokenData.ExpiredAt);

        return Result<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenData.RefreshToken,
        });
    }

    // ─── Refresh Token ───────────────────────────────────────────────────────

    public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
    {
        var rotated = await _refreshTokenService.RotateRefreshTokenAsync(refreshToken);
        if (rotated == null)
            return Result<AuthResponseDto>.Fail("Refresh token is invalid or has expired");
        
        var tokenData = await _refreshTokenService.GetRefreshTokenAsync(rotated.RefreshToken);
        if (tokenData == null)
            return Result<AuthResponseDto>.Fail("Refresh token data not found");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == tokenData.UserId);
        if (user == null)
            return Result<AuthResponseDto>.Fail("User not found");

        var newAccessToken = _jwtTokenService.GenerateToken(user);

        return Result<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = rotated.RefreshToken,
        });
    }

    // ─── Change Password ──────────────────────────────────────────────────────

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return Result.Fail("User not found");

        if (!_passwordHasherService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return Result.Fail("Current password is incorrect");
        
        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync();
        
        await _refreshTokenService.RevokeAllUserTokensAsync(userId);

        return Result.Ok();
    }

    // ─── Get Current User ────────────────────────────────────────────────────

    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(r => r.Id == userId);
        if (user == null)
            return Result<UserDto>.Fail("User not found");

        var userDto = new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
        };

        return Result<UserDto>.Ok(userDto);
    }

    // ─── Forgot Password - Step 1: Request OTP ─────────────────────────────
    public async Task<Result> RequestForgotPasswordOtpAsync(ForgotPasswordRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user == null)
            return Result.Fail("User with this email does not exist");

        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        await _cache.SetStringAsync(
            $"forgot:otp:{normalizedEmail}",
            otp,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }
        );

        await _emailPublisher.SendEmailAsync(new EmailMessage
        {
            To = request.Email,
            Subject = "Feelory Reset Password OTP",
            Body = $"Your OTP verification code to reset password is <strong>{otp}</strong>. It is valid for 5 minutes."
        });

        return Result.Ok();
    }

    // ─── Forgot Password - Step 2: Verify OTP ───────────────────────────────
    public async Task<Result<ForgotPasswordVerifyResponseDto>> VerifyForgotPasswordOtpAsync(VerifyForgotPasswordOtpRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var cachedOtp = await _cache.GetStringAsync($"forgot:otp:{normalizedEmail}");
        if (string.IsNullOrEmpty(cachedOtp) || cachedOtp != request.Otp)
            return Result<ForgotPasswordVerifyResponseDto>.Fail("Invalid or expired OTP");

        var resetToken = Guid.NewGuid().ToString("N");

        await _cache.SetStringAsync(
            $"forgot:verified:{normalizedEmail}",
            resetToken,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) }
        );

        await _cache.RemoveAsync($"forgot:otp:{normalizedEmail}");

        return Result<ForgotPasswordVerifyResponseDto>.Ok(new ForgotPasswordVerifyResponseDto
        {
            ResetToken = resetToken
        });
    }

    // ─── Forgot Password - Step 3: Reset Password ────────────────────────────
    public async Task<Result> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var cachedToken = await _cache.GetStringAsync($"forgot:verified:{normalizedEmail}");
        if (string.IsNullOrEmpty(cachedToken) || cachedToken != request.Token)
            return Result.Fail("Invalid or expired password reset token. Please verify OTP again.");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        if (user == null)
            return Result.Fail("User not found");

        user.PasswordHash = _passwordHasherService.HashPassword(request.NewPassword);
        await _dbContext.SaveChangesAsync();

        await _refreshTokenService.RevokeAllUserTokensAsync(user.Id);

        await _cache.RemoveAsync($"forgot:verified:{normalizedEmail}");

        return Result.Ok();
    }
}