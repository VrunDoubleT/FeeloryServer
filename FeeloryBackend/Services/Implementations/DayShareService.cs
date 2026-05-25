using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class DayShareService : IDayShareService
{
    private readonly AppDbContext _db;
    private readonly DaySharePublisher _publisher;

    public DayShareService(AppDbContext db, DaySharePublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<Result> CreateAsync(Guid currentUserId, CreateDayShareRequestDto dto)
    {
        // 1. Validate privacy type
        if (dto.Privacy != DayShareTypeConstants.Friends &&
            dto.Privacy != DayShareTypeConstants.Custom)
        {
            return Result.Fail("Invalid privacy type.");
        }

        // 2. Validate selected posts belong to user and fall on the given date
        var dayStart = DateTime.SpecifyKind(dto.Date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var dayEnd   = dayStart.AddDays(1);

        var matchedPosts = await _db.Posts
            .Where(p =>
                dto.SelectedPostIds.Contains(p.Id) &&
                p.UserId == currentUserId &&
                p.CreatedAt >= dayStart &&
                p.CreatedAt < dayEnd)
            .Select(p => p.Id)
            .ToListAsync();

        if (matchedPosts.Count != dto.SelectedPostIds.Distinct().Count())
            return Result.Fail("Some posts are invalid, do not belong to you, or are not from the given date.");

        // 3. Resolve viewer list based on privacy
        List<Guid> viewerIds;

        var friendIds = await _db.Friends
            .GetFriendsOfUser(currentUserId)
            .Select(f => f.UserId == currentUserId ? f.FriendId : f.UserId)
            .Distinct()
            .ToListAsync();

        if (dto.Privacy == DayShareTypeConstants.Friends)
        {
            viewerIds = friendIds;
        }
        else // Custom
        {
            if (dto.AllowedUserIds is null || dto.AllowedUserIds.Count == 0)
                return Result.Fail("At least one allowed friend is required for Custom privacy.");

            var allowedDistinct = dto.AllowedUserIds.Distinct().ToList();

            if (allowedDistinct.Any(id => !friendIds.Contains(id)))
                return Result.Fail("Some allowed users are not your friends.");

            viewerIds = allowedDistinct;
        }

        // 4. Persist DayShare
        var dayShare = new DayShare
        {
            Id          = Guid.NewGuid(),
            OwnerId     = currentUserId,
            Description = dto.Description,
            SharedDate  = dto.Date,
            ShareType   = dto.Privacy
        };

        _db.DayShares.Add(dayShare);

        // 5. Persist ordered post references
        var daySharePosts = dto.SelectedPostIds
            .Select((postId, index) => new DaySharePost
            {
                Id           = Guid.NewGuid(),
                DayShareId   = dayShare.Id,
                PostId       = postId,
                DisplayOrder = index + 1
            });

        _db.DaySharePosts.AddRange(daySharePosts);

        // 6. Persist explicit viewers for Custom privacy
        if (dto.Privacy == DayShareTypeConstants.Custom)
        {
            var viewers = viewerIds.Select(viewerId => new DayShareViewer
            {
                Id         = Guid.NewGuid(),
                DayShareId = dayShare.Id,
                ViewerId   = viewerId
            });

            _db.DayShareViewers.AddRange(viewers);
        }

        await _db.SaveChangesAsync();

        // 7. Publish feed event via RabbitMQ
        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action     = "CREATED",
            DayShareId = dayShare.Id,
            ViewerIds  = viewerIds
        });

        return Result.Ok();
    }
}