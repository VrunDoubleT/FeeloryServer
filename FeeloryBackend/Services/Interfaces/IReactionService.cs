using FeeloryBackend.Models.DTOs.Reaction;

namespace FeeloryBackend.Services.Interfaces;

public interface IReactionService
{
    // Add reaction to post
    Task AddAsync(Guid userId, Guid postId, Guid emoteId);

    // Remove reaction
    Task RemoveAsync(Guid userId, Guid postId);

    // Get reactions of post
    Task<List<ReactionDto>> GetByPostAsync(Guid postId);
}