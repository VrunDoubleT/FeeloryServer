using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Responses;

namespace FeeloryBackend.Services.Interfaces;

public interface IDayShareService
{
    Task<Result<DayShareDto>> CreateAsync(
        Guid currentUserId,
        CreateDayShareRequestDto dto);

    Task<Result<DayShareDto>> UpdateAsync(
        Guid currentUserId,
        UpdateDayShareRequestDto dto);

    Task<Result<DayShareDetailDto>> GetByIdAsync(
        Guid currentUserId,
        Guid dayShareId);

    Task<Result> DeleteAsync(
        Guid currentUserId,
        Guid dayShareId);
    
    Task<Result<DayShareDto?>> GetTodayAsync(Guid userId);

    Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetFeedAsync(
        Guid currentUserId,
        CursorPaginationRequest pagination);

    Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetUserFeedAsync(
        Guid currentUserId,
        Guid targetUserId,
        CursorPaginationRequest pagination);
}