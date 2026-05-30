
using FeeloryBackend.Models.DTOs.User;

namespace FeeloryBackend.Services.Interfaces;

public interface IUserService
{
    // Get user profile by id
    Task<UserSummaryDto> GetByIdAsync(Guid userId);
    
    // Update user profile information
    Task UpdateProfileAsync(Guid userId, UpdateUserRequestDto request);
    
    // Search users by keyword
    Task<List<UserSummaryDto>> SearchAsync(string keyword);
}