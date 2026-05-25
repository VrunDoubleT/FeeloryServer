using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Services.Interfaces;

public interface IDayShareService
{
    // Create full day diary share
    Task<Result> CreateAsync(Guid currentUserId, CreateDayShareRequestDto dto);
    
    // Get shared day detail
   // Task<DayShareDto> GetByIdAsync(Guid id);
    
    // Delete shared day
   // Task DeleteAsync(Guid userId, Guid id);
    
    // Get timeline of shared day
    //Task<List<PostDto>> GetTimelineAsync(Guid dayShareId);
}