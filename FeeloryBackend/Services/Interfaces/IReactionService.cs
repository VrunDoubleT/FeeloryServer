using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Reaction;

namespace FeeloryBackend.Services.Interfaces;

public interface IReactionService
{
    Task<Result<ReactionResponseDto>> AddToPostAsync(Guid currentUserId, Guid postId, Guid emoteId);
    Task<Result> RemoveFromPostAsync(Guid currentUserId, Guid postId);
    Task<Result<List<ReactionGroupDto>>> GetByPostAsync(Guid currentUserId, Guid postId);
}