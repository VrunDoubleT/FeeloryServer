using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
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
    private readonly ReactionPublisher _publisher;

    public ReactionService(
        AppDbContext db,
        ReactionPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<Result<ReactionResponseDto>> AddToPostAsync(
        Guid currentUserId,
        Guid postId,
        Guid emoteId)
    {
        var post = await _db.Posts
            .FirstOrDefaultAsync(x =>
                x.Id == postId &&
                x.DeletedAt == null);

        if (post is null)
        {
            return Result<ReactionResponseDto>
                .Fail("Post not found.");
        }

        bool canView = await CanViewPostAsync(
            currentUserId,
            post);

        if (!canView)
        {
            return Result<ReactionResponseDto>
                .Fail("You do not have permission to react to this post.");
        }

        bool emoteOwned = await IsEmoteOwnedByUserAsync(
            currentUserId,
            emoteId);

        if (!emoteOwned)
        {
            return Result<ReactionResponseDto>
                .Fail("You do not own this emote.");
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
            return Result<ReactionResponseDto>
                .Fail("Reaction already exists.");
        }

        var user = await _db.Users
            .Where(x => x.Id == currentUserId)
            .Select(x => new UserSummaryDto
            {
                Id = x.Id,
                DisplayName = x.DisplayName,
                AvatarUrl = x.AvatarUrl
            })
            .FirstAsync();

        var emote = await _db.Emotes
            .Where(x => x.Id == emoteId)
            .Select(x => new EmoteDto
            {
                Id = x.Id,
                Name = x.Name,
                ImageUrl = x.ImageUrl
            })
            .FirstAsync();

        // Notification event
        if (post.UserId != currentUserId)
        {
            await _publisher.PublishNotificationAsync(
                new ReactionMessage
                {
                    Action = ReactionMessage.ActionPostReacted,
                    TargetOwnerId = post.UserId,
                    ReactorId = currentUserId,
                    ReactorName = user.DisplayName,
                    TargetId = postId
                });

            // Task completion trigger
            await _publisher.PublishTaskAsync(
                new TaskReactionMessage
                {
                    UserId = currentUserId,
                    ReactionId = reaction.Id,
                    CreatedAt = DateTime.UtcNow
                });
        }

        return Result<ReactionResponseDto>.Ok(
            new ReactionResponseDto
            {
                ReactionId = reaction.Id,
                Emote = emote,
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
        var post = await _db.Posts
            .FirstOrDefaultAsync(x =>
                x.Id == postId &&
                x.DeletedAt == null);

        if (post is null)
        {
            return Result<List<ReactionGroupDto>>
                .Fail("Post not found.");
        }


        if (post.UserId != currentUserId)
        {
            return Result<List<ReactionGroupDto>>
                .Fail("Only the post owner can view reactions.");
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

        return Result<List<ReactionGroupDto>>
            .Ok(grouped);
    }


    private async Task<bool> CanViewPostAsync(
        Guid currentUserId,
        Post post)
    {
        if (post.UserId == currentUserId)
            return true;

        return post.Privacy switch
        {
            PostPrivacyConstants.Public => true,
            PostPrivacyConstants.Private => false,
            PostPrivacyConstants.Custom =>
                await _db.PostFeeds
                    .AnyAsync(x =>
                        x.PostId == post.Id &&
                        x.ViewerId == currentUserId),

            _ => false
        };  
    }

    private async Task<bool> IsEmoteOwnedByUserAsync(
        Guid userId,
        Guid emoteId)
    {
        bool isDefault = await _db.EmotePackageItems
            .AnyAsync(x =>
                x.EmoteId == emoteId &&
                x.Package.IsDefault);

        if (isDefault)
            return true;


        return await _db.UserPackages
            .AnyAsync(up =>
                up.UserId == userId &&
                _db.EmotePackageItems
                    .Any(ep =>
                        ep.PackageId == up.PackageId &&
                        ep.EmoteId == emoteId));
    }
}