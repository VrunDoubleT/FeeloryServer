using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Reaction;

namespace FeeloryBackend.Services.Interfaces;

public interface IReactionService
{
    Task<Result<ReactionResponseDto>> AddToPostAsync(Guid currentUserId, Guid postId, Guid emoteId);
    Task<Result> RemoveFromPostAsync(Guid currentUserId, Guid postId);
    Task<Result<List<ReactionGroupDto>>> GetByPostAsync(Guid currentUserId, Guid postId);
    
    /// <summary>
    /// Retrieves the emote that a user reacted with on a specific post.
    /// </summary>
    Task<EmoteDto?> GetUserReactionEmoteAsync(Guid userId, Guid postId);
}