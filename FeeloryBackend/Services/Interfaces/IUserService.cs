using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.User;

namespace FeeloryBackend.Services.Interfaces;

public interface IUserService
{
    // Get user profile by id
    Task<UserDto> GetByIdAsync(Guid userId);
    
    // Update user profile information
    Task UpdateProfileAsync(Guid userId, UpdateUserRequestDto request);
    
    // Search users by keyword
    Task<List<UserDto>> SearchAsync(string keyword);
}