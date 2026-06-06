using FeeloryBackend.Commons;
using FeeloryBackend.Data;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Models.DTOs.User;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Reaction;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class ReactionService : IReactionService
{
    private readonly AppDbContext _db;
    private readonly PostReactionPublisher _postReactionPublisher;
    private readonly IPostAccessService _postAccessService;
    private readonly IEmoteService _emoteService;

    public ReactionService(
        AppDbContext db,
        PostReactionPublisher postReactionPublisher,
        IPostAccessService postAccessService,
        IEmoteService emoteService
    ) {
        _db = db;
        _postReactionPublisher = postReactionPublisher;
        _postAccessService = postAccessService;
        _emoteService = emoteService;
    }

    public async Task<Result<ReactionResponseDto>> AddToPostAsync(
        Guid currentUserId,
        Guid postId,
        Guid emoteId)
    {
        var post = await _db.Posts.Where(x => x.Id == postId && x.DeletedAt == null).FirstOrDefaultAsync();
        if (post is null)
        {
            return Result<ReactionResponseDto>.Fail("Post not found");
        }

        if (await _postAccessService.IsPostOwnerAsync(postId, currentUserId))
        {
            return  Result<ReactionResponseDto>.Fail("You cannot react to your own post");
        }
        
        if (!await _postAccessService.CanViewPostAsync(postId, currentUserId))
        {
            return Result<ReactionResponseDto>.Fail("You do not have permission to react to this post");
        }
        
        if (!await _emoteService.HasEmoteAsync(currentUserId, emoteId))
        {
            return Result<ReactionResponseDto>.Fail("You do not own this emote");
        }

        var reaction = await _db.Reactions
            .FirstOrDefaultAsync(x =>
                x.PostId == postId &&
                x.UserId == currentUserId);

        if (reaction is null)
        {
            reaction = new Reaction
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = currentUserId,
                EmoteId = emoteId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reactions.Add(reaction);
        }
        else
        {
            reaction.EmoteId = emoteId;
        }

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.InnerException?.Message);
            return Result<ReactionResponseDto>.Fail("Reaction already exists.");
        }
        
        var emote = await _emoteService.FindByIdAsync(emoteId);
        
        // Notification event
        await _postReactionPublisher.PublishPostReactionAddedAsync(new PostReactionAddedMessage()
        {
            OwnerId = post.UserId,
            ReactorId = currentUserId,
            PostId = postId,
            EmoteId = emoteId
        });

        return Result<ReactionResponseDto>.Ok(
            new ReactionResponseDto
            {
                ReactionId = reaction.Id,
                Emote = emote!,
                CreatedAt = reaction.CreatedAt
            });
    }

    public async Task<Result> RemoveFromPostAsync(
        Guid currentUserId,
        Guid postId)
    {
        var reaction = await _db.Reactions
            .FirstOrDefaultAsync(x =>
                x.PostId == postId &&
                x.UserId == currentUserId);

        if (reaction is null)
        {
            return Result.Fail("Reaction not found.");
        }

        _db.Reactions.Remove(reaction);

        await _db.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result<List<ReactionGroupDto>>> GetByPostAsync(
        Guid currentUserId,
        Guid postId)
    {
        var post = await _db.Posts.Where(x => x.Id == postId && x.DeletedAt == null).FirstOrDefaultAsync();

        if (post is null)
        {
            return Result<List<ReactionGroupDto>>.Fail("Post not found.");
        }

        if (post.UserId != currentUserId)
        {
            return Result<List<ReactionGroupDto>>.Fail("Only the post owner can view reactions.");
        }

        var reactions = await _db.Reactions
            .Where(x => x.PostId == postId)
            .Select(x => new
            {
                UserId = x.User.Id,
                DisplayName = x.User.DisplayName,
                AvatarUrl = x.User.AvatarUrl,

                EmoteId = x.Emote.Id,
                EmoteName = x.Emote.Name,
                EmoteImageUrl = x.Emote.ImageUrl,

                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        var grouped = reactions
            .GroupBy(x => x.EmoteId)
            .Select(g => new ReactionGroupDto
            {
                Emote = new EmoteDto
                {
                    Id = g.Key,
                    Name = g.First().EmoteName,
                    ImageUrl = g.First().EmoteImageUrl
                },

                Count = g.Count(),

                Users = g
                    .Select(x => new ReactionUserDto
                    {
                        User = new UserSummaryDto
                        {
                            Id = x.UserId,
                            DisplayName = x.DisplayName,
                            AvatarUrl = x.AvatarUrl
                        },

                        CreatedAt = x.CreatedAt
                    })
                    .OrderByDescending(x => x.CreatedAt)
                    .ToList()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return Result<List<ReactionGroupDto>>.Ok(grouped);
    }
    
    /// <summary>
    /// Retrieves the emote that a user reacted with on a specific post.
    /// Returns null if the user has not reacted to the post.
    /// </summary>
    /// <param name="userId">
    /// The unique identifier of the user.
    /// </param>
    /// <param name="postId">
    /// The unique identifier of the post.
    /// </param>
    /// <returns>
    /// An <see cref="EmoteDto"/> if the user has reacted to the post;
    /// otherwise, null.
    /// </returns>
    public async Task<EmoteDto?> GetUserReactionEmoteAsync(
        Guid userId,
        Guid postId)
    {
        return await _db.Reactions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.PostId == postId)
            .Select(x => new EmoteDto
            {
                Id = x.Emote.Id,
                Name = x.Emote.Name,
                ImageUrl = x.Emote.ImageUrl
            })
            .FirstOrDefaultAsync();
    }
}