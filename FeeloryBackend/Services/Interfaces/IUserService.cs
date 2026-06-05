using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.User;
using FeeloryBackend.Responses;

namespace FeeloryBackend.Services.Interfaces;

public interface IUserService
{
    Task<Result<UserProfileDto>> GetProfileAsync(Guid currentUserId, Guid targetUserId);
    Task<Result<UserProfileDto>> UpdateProfileAsync(Guid currentUserId, UpdateUserRequestDto request);
    Task<Result<CursorPaginationResponse<UserProfileDto>>> SearchByDisplayNameAsync(Guid currentUserId, string q, CursorPaginationRequest request);
    Task<Result<CursorPaginationResponse<UserProfileDto>>> SearchByUsernameAsync(Guid currentUserId, string username, CursorPaginationRequest request);
}