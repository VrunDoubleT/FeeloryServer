using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Services.Interfaces;

public interface IEmoteService
{
    // Get all emotes
    Task<List<EmoteDto>> GetAllAsync();
    
    // Get emote by id
    Task<EmoteDto> GetByIdAsync(Guid emoteId);
    
    // Get user owned emotes
    Task<List<EmoteDto>> GetUserEmotesAsync(Guid userId);
}