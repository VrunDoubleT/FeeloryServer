using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Services.Interfaces;

public interface IEmoteService
{
    Task<Result<Dictionary<string, List<EmoteDto>>>> GetAllGroupedAsync();
    Task<Result<EmoteDto>> GetByIdAsync(Guid id);
    Task<Result<List<UserEmoteDto>>> GetUserEmotesAsync(Guid userId);
    Task<Result<List<EmoteDto>>> GetRecentEmotesAsync(Guid userId, int limit);
}