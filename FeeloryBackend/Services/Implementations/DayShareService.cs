using System.Text;
using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Responses;
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
        var dayEnd = dayStart.AddDays(1);

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

        // 4. Check duplicate DayShare on the same day
        bool exists = await _db.DayShares.AnyAsync(x =>
            x.OwnerId == currentUserId &&
            x.SharedDate.Date == dto.Date.ToDateTime(TimeOnly.MinValue).Date &&
            x.DeletedAt == null);

        if (exists)
        {
            return Result.Fail("You already shared this day.");
        }

        // 5. Persist DayShare
        var dayShare = new DayShare
        {
            Id = Guid.NewGuid(),
            OwnerId = currentUserId,
            Description = dto.Description,
            SharedDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue),
                DateTimeKind.Utc),
            ShareType = dto.Privacy
        };

        _db.DayShares.Add(dayShare);

        // 6. Persist ordered post references
        var now = DateTime.UtcNow;
        var daySharePosts = dto.SelectedPostIds
            .Select((postId, index) => new DaySharePost
            {
                Id = Guid.NewGuid(),
                DayShareId = dayShare.Id,
                PostId = postId,
                DisplayOrder = index + 1,
                CreatedAt = now
            });

        _db.DaySharePosts.AddRange(daySharePosts);

        // 7. Persist explicit viewers for Custom privacy
        if (dto.Privacy == DayShareTypeConstants.Custom)
        {
            var viewers = viewerIds.Select(viewerId => new DayShareViewer
            {
                Id = Guid.NewGuid(),
                DayShareId = dayShare.Id,
                ViewerId = viewerId
            });

            _db.DayShareViewers.AddRange(viewers);
        }

        await _db.SaveChangesAsync();

        // 8. Publish feed event via RabbitMQ
        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action = DayShareFeedMessage.ActionCreated,
            DayShareId = dayShare.Id,
            ViewerIds = viewerIds
        });

        return Result.Ok();
    }

    public async Task<Result> UpdateAsync(Guid currentUserId, UpdateDayShareRequestDto dto)
    {
        // 1. Validate privacy
        if (dto.Privacy != DayShareTypeConstants.Friends &&
            dto.Privacy != DayShareTypeConstants.Custom)
            return Result.Fail("Invalid privacy type.");

        // 2. Find DayShare and verify ownership
        var dayShare = await _db.DayShares
            .FirstOrDefaultAsync(x =>
                x.Id == dto.DayShareId &&
                x.DeletedAt == null);

        if (dayShare is null)
            return Result.Fail("DayShare not found.");

        if (dayShare.OwnerId != currentUserId)
            return Result.Fail("You are not the owner of this DayShare.");

        // 3. Validate and update posts if provided
        if (dto.SelectedPostIds is not null &&
            dto.SelectedPostIds.Count > 0)
        {
            var dayStart = DateTime.SpecifyKind(
                dayShare.SharedDate.Date,
                DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);

            var distinctPostIds = dto.SelectedPostIds.Distinct().ToList();

            var matchedPosts = await _db.Posts
                .Where(p =>
                    distinctPostIds.Contains(p.Id) &&
                    p.UserId == currentUserId &&
                    p.CreatedAt >= dayStart &&
                    p.CreatedAt < dayEnd)
                .Select(p => p.Id)
                .ToListAsync();

            if (matchedPosts.Count != distinctPostIds.Count)
                return Result.Fail(
                    "Some posts are invalid, do not belong to you, or are not from the given date.");

            var oldDaySharePosts = await _db.DaySharePosts
                .Where(x => x.DayShareId == dayShare.Id)
                .ToListAsync();

            var oldPostIds = oldDaySharePosts
                .Select(x => x.PostId)
                .ToList();

            var removedPostIds = oldPostIds.Except(distinctPostIds).ToList();
            var addedPostIds = distinctPostIds.Except(oldPostIds).ToList();

            // Remove dropped posts
            if (removedPostIds.Any())
            {
                _db.DaySharePosts.RemoveRange(
                    oldDaySharePosts.Where(x =>
                        removedPostIds.Contains(x.PostId)));
            }

            // Add new posts
            var newDaySharePosts = addedPostIds
                .Select(postId => new DaySharePost
                {
                    Id = Guid.NewGuid(),
                    DayShareId = dayShare.Id,
                    PostId = postId,
                    DisplayOrder = 0
                })
                .ToList();

            if (addedPostIds.Any())
                _db.DaySharePosts.AddRange(newDaySharePosts);

            // Reorder all posts according to SelectedPostIds order
            var allPosts = oldDaySharePosts
                .Where(x => !removedPostIds.Contains(x.PostId))
                .Concat(newDaySharePosts)
                .ToList();

            foreach (var post in allPosts)
            {
                post.DisplayOrder = distinctPostIds.IndexOf(post.PostId) + 1;
            }
        }

        // 4. Get old viewer list from correct source
        var oldViewerIds = dayShare.ShareType == DayShareTypeConstants.Custom
            ? await _db.DayShareViewers
                .Where(x => x.DayShareId == dayShare.Id)
                .Select(x => x.ViewerId)
                .ToListAsync()
            : await _db.Friends
                .GetFriendsOfUser(currentUserId)
                .Select(x => x.UserId == currentUserId ? x.FriendId : x.UserId)
                .Distinct()
                .ToListAsync();

        // 5. Get new viewer list based on updated privacy
        var friendIds = await _db.Friends
            .GetFriendsOfUser(currentUserId)
            .Select(x => x.UserId == currentUserId ? x.FriendId : x.UserId)
            .Distinct()
            .ToListAsync();

        List<Guid> newViewerIds;

        if (dto.Privacy == DayShareTypeConstants.Friends)
        {
            newViewerIds = friendIds;
        }
        else
        {
            if (dto.AllowedUserIds is null || dto.AllowedUserIds.Count == 0)
                return Result.Fail(
                    "At least one allowed friend is required for Custom privacy.");

            var allowedDistinct = dto.AllowedUserIds.Distinct().ToList();

            if (allowedDistinct.Any(id => !friendIds.Contains(id)))
                return Result.Fail("Some allowed users are not your friends.");

            newViewerIds = allowedDistinct;
        }

        // 6. Diff old vs new viewers
        var removedViewerIds = oldViewerIds.Except(newViewerIds).ToList();
        var addedViewerIds = newViewerIds.Except(oldViewerIds).ToList();

        // 7. Update DayShare fields
        dayShare.Description = dto.Description;
        dayShare.ShareType = dto.Privacy;
        dayShare.UpdatedAt = DateTime.UtcNow;

        // 8. Update DayShareViewers
        var oldViewers = await _db.DayShareViewers
            .Where(x => x.DayShareId == dayShare.Id)
            .ToListAsync();

        _db.DayShareViewers.RemoveRange(oldViewers);

        if (dto.Privacy == DayShareTypeConstants.Custom)
        {
            _db.DayShareViewers.AddRange(newViewerIds.Select(viewerId =>
                new DayShareViewer
                {
                    Id = Guid.NewGuid(),
                    DayShareId = dayShare.Id,
                    ViewerId = viewerId
                }));
        }


        // 9. Save all changes at once
        await _db.SaveChangesAsync();

        // 10. Publish feed change events
        if (removedViewerIds.Any())
        {
            await _publisher.PublishAsync(new DayShareFeedMessage
            {
                Action = DayShareFeedMessage.ActionRemoved,
                DayShareId = dayShare.Id,
                ViewerIds = removedViewerIds
            });
        }

        if (addedViewerIds.Any())
        {
            await _publisher.PublishAsync(new DayShareFeedMessage
            {
                Action = DayShareFeedMessage.ActionAdded,
                DayShareId = dayShare.Id,
                ViewerIds = addedViewerIds
            });
        }

        return Result.Ok();
    }

    public async Task<Result<DayShareDetailDto>> GetByIdAsync(Guid currentUserId, Guid dayShareId)
    {
        // 1. Fetch DayShare
        var dayShare = await _db.DayShares
            .Where(x => x.Id == dayShareId && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.SharedDate,
                x.Description,
                x.ShareType,
                x.OwnerId,

                Owner = new DayShareOwnerDto
                {
                    Id = x.Owner.Id,
                    DisplayName = x.Owner.DisplayName,
                    AvatarUrl = x.Owner.AvatarUrl
                }
            })
            .FirstOrDefaultAsync();

        if (dayShare is null)
            return Result<DayShareDetailDto>.Fail("DayShare not found.");

        // 2. Check permission: owner OR in feed
        bool isOwner = dayShare.OwnerId == currentUserId;

        if (!isOwner)
        {
            bool inFeed = await _db.DayShareFeeds
                .AnyAsync(x =>
                    x.DayShareId == dayShareId &&
                    x.ViewerId == currentUserId);

            if (!inFeed)
                return Result<DayShareDetailDto>.Fail(
                    "You do not have permission to view this DayShare.");
        }

        // 3. Fetch posts ordered by DisplayOrder
        var posts = await _db.DaySharePosts
            .Where(x => x.DayShareId == dayShareId)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new DaySharePostItemDto
            {
                Id = x.Post.Id,
                ImageUrl = x.Post.ImageUrl,
                Description = x.Post.Description,
                MoodEmote = x.Post.MoodEmote == null
                    ? null
                    : new DayShareMoodEmoteDto
                    {
                        Id = x.Post.MoodEmote.Id,
                        Name = x.Post.MoodEmote.Name,
                        ImageUrl = x.Post.MoodEmote.ImageUrl
                    },
                CreatedAt = x.Post.CreatedAt
            })
            .ToListAsync();

        return Result<DayShareDetailDto>.Ok(new DayShareDetailDto
        {
            Id = dayShare.Id,
            Date = DateOnly.FromDateTime(dayShare.SharedDate),
            Description = dayShare.Description,
            Privacy = dayShare.ShareType,
            Owner = dayShare.Owner,
            Posts = posts,
        });
    }


    public async Task<Result> DeleteAsync(
        Guid currentUserId,
        Guid dayShareId)
    {
        // 1. Find DayShare and verify ownership
        var dayShare = await _db.DayShares
            .FirstOrDefaultAsync(x =>
                x.Id == dayShareId &&
                x.DeletedAt == null);

        if (dayShare is null)
            return Result.Fail("DayShare not found.");

        if (dayShare.OwnerId != currentUserId)
            return Result.Fail(
                "You are not the owner of this DayShare.");

        // 2. Soft delete
        dayShare.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // 3. Publish event to remove from all feeds
        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action = DayShareFeedMessage.ActionDeleted,
            DayShareId = dayShare.Id,
            //  ViewerIds  = new List<Guid>()
        });

        return Result.Ok();
    }

    public async Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetFeedAsync(
        Guid currentUserId,
        string? cursor,
        int pageSize)
    {
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 50) pageSize = 50;

        // Decode cursor to get the last PostedAt timestamp
        DateTime? cursorTime = null;

        if (!string.IsNullOrEmpty(cursor))
        {
            var decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(cursor));

            if (DateTime.TryParse(decoded, out var parsed))
                cursorTime = parsed;
        }

        var query = _db.DayShareFeeds
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.DayShare.DeletedAt == null &&
                (cursorTime == null || x.PostedAt < cursorTime))
            .OrderByDescending(x => x.PostedAt);

        // Fetch pageSize + 1 to determine if more data exists
        var items = await query
            .Take(pageSize + 1)
            .Select(x => new DayShareFeedItemDto
            {
                DayShareId = x.DayShareId,
                Date = DateOnly.FromDateTime(x.DayShare.SharedDate),
                Description = x.DayShare.Description,
                Owner = new DayShareOwnerDto
                {
                    Id = x.DayShare.Owner.Id,
                    DisplayName = x.DayShare.Owner.DisplayName,
                    AvatarUrl = x.DayShare.Owner.AvatarUrl
                },
                PostCount = x.DayShare.DaySharePosts.Count,
                CreatedAt = x.PostedAt
            })
            .ToListAsync();

        var hasNextPage = items.Count > pageSize;

        if (hasNextPage)
            items = items.Take(pageSize).ToList();

        // Encode next cursor from last item's PostedAt
        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = items.Last();
            var cursorStr = lastItem.CreatedAt.ToString("O");
            nextCursor = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(cursorStr));
        }

        return Result<CursorPaginationResponse<DayShareFeedItemDto>>.Ok(
            new CursorPaginationResponse<DayShareFeedItemDto>(
                items,
                nextCursor,
                hasNextPage));
    }
}