using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Services.Interfaces;

public interface IEmoteService
{
    Task<Result<Dictionary<string, List<EmoteDto>>>> GetAllGroupedAsync();
    Task<Result<EmoteDto>> GetByIdAsync(Guid id);
    Task<Result<Dictionary<string, List<UserEmoteDto>>>> GetUserEmotesAsync(Guid userId);
    Task<Result<List<EmoteDto>>> GetRecentEmotesAsync(Guid userId, int limit);
    Task<bool> HasEmoteAsync(Guid userId, Guid emoteId);
    
    /// <summary>
    /// Retrieves an emote by its identifier.
    /// Returns null if the emote does not exist.
    /// </summary>
    Task<EmoteDto?> FindByIdAsync(Guid id);
}