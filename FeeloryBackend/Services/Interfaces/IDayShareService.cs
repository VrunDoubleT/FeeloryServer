using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Models.DTOs.Post;

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
    Task<Result<DayShareFeedPagedDto>> GetFeedAsync( Guid currentUserId, int page, int pageSize);
}