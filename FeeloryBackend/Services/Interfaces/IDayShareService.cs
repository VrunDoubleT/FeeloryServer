using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Models.DTOs.Post;
using FeeloryBackend.Responses;

namespace FeeloryBackend.Services.Interfaces;

public interface IDayShareService
{
    // Create full day diary share
    Task<Result> CreateAsync(Guid currentUserId, CreateDayShareRequestDto dto);

    // Update shareday
    Task<Result> UpdateAsync(Guid currentUserId, UpdateDayShareRequestDto dto);

    // Get shared day detail
    Task<Result<DayShareDetailDto>> GetByIdAsync(Guid currentUserId, Guid dayShareId);

    // Delete shared day
    Task<Result> DeleteAsync(Guid currentUserId, Guid dayShareId);

    // Get feed
    Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetFeedAsync(
        Guid currentUserId,
        string? cursor,
        int pageSize);
    
    // Get feed for user
    Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetUserFeedAsync(
        Guid currentUserId,
        Guid targetUserId,
        string? cursor,
        int pageSize);
}