using System.Text;
using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Messages.DayShares;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class DayShareService : IDayShareService
{
    private readonly AppDbContext _db;
    private readonly DaySharePublisher _publisher;
    private readonly IDayShareAccessService _dayShareAccessService;
    private readonly IPostService _postService;

    public DayShareService(AppDbContext db, DaySharePublisher publisher, IDayShareAccessService dayShareAccessService,
        IPostService postService)
    {
        _db = db;
        _publisher = publisher;
        _dayShareAccessService = dayShareAccessService;
        _postService = postService;
    }

    /// <summary>
    /// Creates a new DayShare.
    /// </summary>
    public async Task<Result<DayShareDto>> CreateAsync(
        Guid currentUserId,
        CreateDayShareRequestDto dto)
    {
        // Build date range
        var dayStart = DateTime.SpecifyKind(
            dto.Date.ToDateTime(TimeOnly.MinValue),
            DateTimeKind.Utc);

        var dayEnd = dayStart.AddDays(1);

        // Validate selected posts
        var isValidPosts = await ValidatePostsAsync(
            currentUserId,
            dto.SelectedPostIds,
            dayStart,
            dayEnd);

        if (!isValidPosts)
        {
            return Result<DayShareDto>.Fail(
                "Some posts are invalid, do not belong to you, or are not from the given date.");
        }

        // Resolve viewers
        var viewerResult = await ResolveViewerIdsAsync(
            currentUserId,
            dto.Privacy,
            dto.AllowedUserIds);

        if (!viewerResult.IsSuccess)
        {
            return Result<DayShareDto>.Fail(viewerResult.Error!);
        }

        var viewerIds = viewerResult.Data!;
        
        // Add owner
        viewerIds.Add(currentUserId);

        // Prevent duplicate share on the same date
        bool exists = await _db.DayShares.AnyAsync(x =>
            x.OwnerId == currentUserId &&
            x.SharedDate.Date == dayStart.Date &&
            x.DeletedAt == null);

        if (exists)
        {
            return Result<DayShareDto>.Fail("You already shared this day.");
        }

        // Create DayShare
        var dayShare = new DayShare
        {
            Id = Guid.NewGuid(),
            OwnerId = currentUserId,
            Description = dto.Description,
            SharedDate = dayStart,
            ShareType = dto.Privacy
        };

        _db.DayShares.Add(dayShare);

        // Create DaySharePosts
        var daySharePosts = await CreateDaySharePostsAsync(
            dayShare.Id,
            dto.SelectedPostIds);

        _db.DaySharePosts.AddRange(daySharePosts);

        await _db.SaveChangesAsync();

        // Publish feed event
        await _publisher.PublishDayShareCreatedAsync(new DayShareCreatedMessage()
        {
            AuthorId = currentUserId,
            DayShareId = dayShare.Id,
            RecipientIds = viewerIds
        });

        return Result<DayShareDto>.Ok(ToDto(dayShare));
    }

    /// <summary>
    /// Updates an existing DayShare.
    /// </summary>
    public async Task<Result<DayShareDto>> UpdateAsync(
        Guid currentUserId,
        UpdateDayShareRequestDto dto)
    {
        // Find DayShare
        var dayShare = await _db.DayShares
            .FirstOrDefaultAsync(x =>
                x.Id == dto.DayShareId &&
                x.DeletedAt == null);

        if (dayShare is null)
        {
            return Result<DayShareDto>.Fail("DayShare not found.");
        }

        // Verify ownership
        if (dayShare.OwnerId != currentUserId)
        {
            return Result<DayShareDto>.Fail("You are not the owner of this DayShare.");
        }

        // Update posts if provided
        if (dto.SelectedPostIds is { Count: > 0 })
        {
            var distinctPostIds = dto.SelectedPostIds
                .Distinct()
                .ToList();

            var dayStart = DateTime.SpecifyKind(
                dayShare.SharedDate.Date,
                DateTimeKind.Utc);

            var dayEnd = dayStart.AddDays(1);

            var isValidPosts = await ValidatePostsAsync(
                currentUserId,
                distinctPostIds,
                dayStart,
                dayEnd);

            if (!isValidPosts)
            {
                return Result<DayShareDto>.Fail(
                    "Some posts are invalid, do not belong to you, or are not from the given date.");
            }

            await SyncDaySharePostsAsync(
                dayShare.Id,
                distinctPostIds);
        }

        // Resolve viewers
        var viewerResult = await ResolveViewerIdsAsync(
            currentUserId,
            dto.Privacy,
            dto.AllowedUserIds);

        if (!viewerResult.IsSuccess)
        {
            return Result<DayShareDto>.Fail(viewerResult.Error!);
        }

        var newViewerIds = viewerResult.Data;

        // Get existing viewers
        var oldViewerIds = await _db.DayShareFeeds
            .Where(x => x.DayShareId == dayShare.Id)
            .Select(x => x.ViewerId)
            .ToListAsync();

        // Calculate differences
        var removedViewerIds = oldViewerIds
            .Where(x => x != currentUserId)
            .Except(newViewerIds!)
            .ToList();

        var addedViewerIds = newViewerIds!
            .Except(oldViewerIds)
            .ToList();

        // Update DayShare
        dayShare.Description = dto.Description;
        dayShare.ShareType = dto.Privacy;
        dayShare.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        
        await _publisher.PublishDayShareUpdatedAsync(new DayShareUpdatedMessage()
        {
            AuthorId = currentUserId,
            DayShareId = dayShare.Id,
            AddedViewerIds = addedViewerIds,
            RemovedViewerIds = removedViewerIds
        });

        return Result<DayShareDto>.Ok(ToDto(dayShare));
    }

    /// <summary>
    /// Gets DayShare detail by id using PostService only.
    /// Permission and reaction logic are handled inside PostService.
    /// </summary>
    public async Task<Result<DayShareDetailDto>> GetByIdAsync(
        Guid currentUserId,
        Guid dayShareId)
    {
        // 1. Fetch DayShare
        var dayShare = await _db.DayShares
            .AsNoTracking()
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
        {
            return Result<DayShareDetailDto>.Fail("DayShare not found");
        }

        // 2. Permission check ONLY for DayShare
        if (dayShare.OwnerId != currentUserId)
        {
            bool canView = await _dayShareAccessService
                .CanViewDayShareAsync(dayShare.Id, currentUserId);

            if (!canView)
            {
                return Result<DayShareDetailDto>.Fail("You do not have permission to view this DayShare");
            }
        }

        // 3. Get ordered PostIds
        var postIds = await _db.DaySharePosts
            .Where(x => x.DayShareId == dayShareId)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => x.PostId)
            .ToListAsync();

        // 4. Delegate ALL post logic to PostService
        var postItems = new List<DaySharePostItemDto>();

        foreach (var postId in postIds)
        {
            var result = await _postService.GetByIdAsync(currentUserId, postId);

            if (!result.IsSuccess || result.Data == null)
                continue;

            var post = result.Data;

            postItems.Add(new DaySharePostItemDto
            {
                Id = post.Post.Id,
                ImageUrl = post.Post.ImageUrl,
                Description = post.Post.Description,
                MoodEmote = post.Post.MoodEmote,
                CreatedAt = post.Post.CreatedAt,

                MyReaction = post.Emote,
                Reactions = post.Reactions
            });
        }
        
        var allowedUserIds = new List<Guid>();

        if (dayShare.ShareType == DayShareTypeConstants.Custom)
        {
            allowedUserIds = await _db.DayShareFeeds
                .Where(x => x.DayShareId == dayShareId &&
                            x.ViewerId != dayShare.OwnerId)
                .Select(x => x.ViewerId)
                .ToListAsync();
        }

        // 5. Return result
        return Result<DayShareDetailDto>.Ok(new DayShareDetailDto
        {
            Id = dayShare.Id,
            Date = DateOnly.FromDateTime(dayShare.SharedDate),
            Description = dayShare.Description,
            Privacy = dayShare.ShareType,
            Owner = dayShare.Owner,
            Posts = postItems,
            CreatedAt = dayShare.SharedDate,
            AllowedUserIds = allowedUserIds
        });
    }

    public async Task<Result> DeleteAsync(
        Guid currentUserId,
        Guid dayShareId)
    {
        var dayShare = await _db.DayShares
            .FirstOrDefaultAsync(x =>
                x.Id == dayShareId &&
                x.DeletedAt == null);

        if (dayShare is null)
            return Result.Fail("DayShare not found.");

        if (dayShare.OwnerId != currentUserId)
            return Result.Fail("You are not the owner of this DayShare.");

        dayShare.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _publisher.PublishDayShareDeletedAsync(new DayShareDeletedMessage()
        {
            DayShareId = dayShare.Id
        });

        return Result.Ok();
    }

    public async Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetFeedAsync(
        Guid currentUserId,
        CursorPaginationRequest pagination)
    {
        var pageSize = Math.Clamp(pagination.PageSize, 1, 50);

        var query = _db.DayShareFeeds
            .AsNoTracking()
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.DayShare.DeletedAt == null)
            .OrderByDescending(x => x.PostedAt)
            .ThenByDescending(x => x.DayShareId)
            .Select(x => new DayShareFeedProjection
            {
                DayShareId = x.DayShareId,
                PostedAt = x.PostedAt,
                SharedDate = x.DayShare.SharedDate,
                Description = x.DayShare.Description,
                Owner = new DayShareOwnerDto
                {
                    Id = x.DayShare.Owner.Id,
                    DisplayName = x.DayShare.Owner.DisplayName,
                    AvatarUrl = x.DayShare.Owner.AvatarUrl
                },
                PostCount = x.DayShare.DaySharePosts.Count
            });

        var response = await BuildFeedResponseAsync(
            query, pageSize, pagination.Cursor,
            last => CursorHelper.Encode(last.PostedAt, last.DayShareId));

        return Result<CursorPaginationResponse<DayShareFeedItemDto>>.Ok(response);
    }

    public async Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetUserFeedAsync(
        Guid currentUserId,
        Guid targetUserId,
        CursorPaginationRequest pagination)
    {
        var pageSize = Math.Clamp(pagination.PageSize, 1, 50);
        var isOwner = currentUserId == targetUserId;

        var query = _db.DayShares
            .AsNoTracking()
            .Where(x =>
                x.OwnerId == targetUserId &&
                x.DeletedAt == null &&
                (isOwner || x.DayShareFeeds.Any(f => f.ViewerId == currentUserId)))
            .OrderByDescending(x => x.SharedDate)
            .ThenByDescending(x => x.Id)
            .Select(x => new DayShareFeedProjection
            {
                DayShareId = x.Id,
                PostedAt = x.SharedDate,
                SharedDate = x.SharedDate,
                Description = x.Description,
                Owner = new DayShareOwnerDto
                {
                    Id = x.Owner.Id,
                    DisplayName = x.Owner.DisplayName,
                    AvatarUrl = x.Owner.AvatarUrl
                },
                PostCount = x.DaySharePosts.Count
            });

        var response = await BuildFeedResponseAsync(
            query, pageSize, pagination.Cursor,
            last => CursorHelper.Encode(last.PostedAt, last.DayShareId));

        return Result<CursorPaginationResponse<DayShareFeedItemDto>>.Ok(response);
    }

    public async Task<Result<DayShareDto?>> GetTodayAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;

        var dayShare = await _db.DayShares
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.OwnerId == userId &&
                x.DeletedAt == null &&
                x.SharedDate.Date == today);

        if (dayShare == null)
        {
            return Result<DayShareDto?>.Ok(null);
        }

        return Result<DayShareDto?>.Ok(ToDto(dayShare));
    }
    
    // -----------------------------------
    // Private helper
    // -----------------------------------

    /// <summary>
    /// Resolves the list of viewers based on privacy settings.
    /// </summary>
    private async Task<Result<List<Guid>>> ResolveViewerIdsAsync(
        Guid currentUserId,
        string privacy,
        List<Guid>? allowedUserIds)
    {
        var friendIds = await _db.Friends
            .GetFriendIdsOfUserAsync(currentUserId);

        if (privacy == DayShareTypeConstants.Friends)
        {
            return Result<List<Guid>>.Ok(friendIds);
        }

        var allowedDistinct = allowedUserIds!
            .Distinct()
            .ToList();

        if (allowedDistinct.Any(id => !friendIds.Contains(id)))
        {
            return Result<List<Guid>>.Fail(
                "Some allowed users are not your friends.");
        }

        return Result<List<Guid>>.Ok(allowedDistinct);
    }

    /// <summary>
    /// Validates that all selected posts belong to the user
    /// and were created on the specified date.
    /// </summary>
    private async Task<bool> ValidatePostsAsync(
        Guid currentUserId,
        List<Guid> selectedPostIds,
        DateTime dayStart,
        DateTime dayEnd)
    {
        var matchedPosts = await _db.Posts
            .Where(p =>
                selectedPostIds.Contains(p.Id) &&
                p.UserId == currentUserId &&
                p.CreatedAt >= dayStart &&
                p.CreatedAt < dayEnd &&
                p.DeletedAt == null)
            .Select(p => p.Id)
            .ToListAsync();

        return matchedPosts.Count == selectedPostIds.Distinct().Count();
    }

    /// <summary>
    /// Converts a DayShare entity to DTO.
    /// </summary>
    private static DayShareDto ToDto(DayShare dayShare)
    {
        return new DayShareDto
        {
            Id = dayShare.Id,
            OwnerId = dayShare.OwnerId,
            Description = dayShare.Description,
            SharedDate = dayShare.SharedDate,
            ShareType = dayShare.ShareType
        };
    }

    /// <summary>
    /// Creates ordered DaySharePost entities based on Post.CreatedAt.
    /// Earlier posts get smaller DisplayOrder.
    /// </summary>
    private async Task<List<DaySharePost>> CreateDaySharePostsAsync(
        Guid dayShareId,
        List<Guid> postIds)
    {
        // Get posts with their creation time
        var posts = await _db.Posts
            .Where(p => postIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.CreatedAt
            })
            .ToListAsync();

        // Order by creation time (ascending: oldest first)
        var orderedPosts = posts
            .OrderBy(p => p.CreatedAt)
            .ToList();

        var now = DateTime.UtcNow;

        return orderedPosts
            .Select((post, index) => new DaySharePost
            {
                Id = Guid.NewGuid(),
                DayShareId = dayShareId,
                PostId = post.Id,
                DisplayOrder = index + 1,
                CreatedAt = now
            })
            .ToList();
    }

    /// <summary>
    /// Synchronizes DayShare posts.
    /// DisplayOrder is based on Post.CreatedAt (earlier posts first).
    /// </summary>
    private async System.Threading.Tasks.Task SyncDaySharePostsAsync(
        Guid dayShareId,
        List<Guid> newPostIds)
    {
        // Get existing DaySharePosts
        var existingDaySharePosts = await _db.DaySharePosts
            .Where(x => x.DayShareId == dayShareId)
            .ToListAsync();

        var existingPostIds = existingDaySharePosts
            .Select(x => x.PostId)
            .ToHashSet();

        var newPostIdSet = newPostIds.ToHashSet();

        // Remove posts not in request
        var removedPosts = existingDaySharePosts
            .Where(x => !newPostIdSet.Contains(x.PostId))
            .ToList();

        if (removedPosts.Any())
        {
            _db.DaySharePosts.RemoveRange(removedPosts);
        }

        // Add new posts
        var addedPosts = newPostIds
            .Where(postId => !existingPostIds.Contains(postId))
            .Select(postId => new DaySharePost
            {
                Id = Guid.NewGuid(),
                DayShareId = dayShareId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (addedPosts.Any())
        {
            _db.DaySharePosts.AddRange(addedPosts);
        }

        // Merge all active posts (remaining + added)
        var activePostIds = existingDaySharePosts
            .Where(x => !removedPosts.Any(r => r.Id == x.Id))
            .Select(x => x.PostId)
            .Concat(addedPosts.Select(x => x.PostId))
            .ToList();

        // Load Post CreatedAt from DB
        var postTimes = await _db.Posts
            .Where(p => activePostIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.CreatedAt
            })
            .ToListAsync();

        // Create lookup: PostId → CreatedAt
        var orderLookup = postTimes
            .OrderBy(p => p.CreatedAt)
            .Select((p, index) => new
            {
                p.Id,
                Order = index + 1
            })
            .ToDictionary(x => x.Id, x => x.Order);

        // Update remaining posts order
        var remainingPosts = existingDaySharePosts
            .Where(x => !removedPosts.Any(r => r.Id == x.Id))
            .ToList();

        foreach (var post in remainingPosts)
        {
            if (orderLookup.TryGetValue(post.PostId, out var order))
            {
                post.DisplayOrder = order;
            }
        }

        // Update added posts order
        foreach (var post in addedPosts)
        {
            if (orderLookup.TryGetValue(post.PostId, out var order))
            {
                post.DisplayOrder = order;
            }
        }
    }
    
    

    private async Task<CursorPaginationResponse<DayShareFeedItemDto>> BuildFeedResponseAsync(
        IQueryable<DayShareFeedProjection> query,
        int pageSize,
        string? cursor,
        Func<DayShareFeedProjection, string> encodeNextCursor)
    {
        // 1. Apply cursor
        if (!string.IsNullOrWhiteSpace(cursor))
        {
            var (cursorTime, cursorId) = CursorHelper.Decode(cursor);

            query = query.Where(x =>
                x.PostedAt < cursorTime ||
                (x.PostedAt == cursorTime && x.DayShareId.CompareTo(cursorId) < 0));
        }

        // 2. Fetch
        var feedEntries = await query
            .Take(pageSize + 1)
            .ToListAsync();

        // 3. hasNextPage
        var hasNextPage = feedEntries.Count > pageSize;
        feedEntries = feedEntries.Take(pageSize).ToList();

        // 4. Fetch posts
        var dayShareIds = feedEntries.Select(x => x.DayShareId).ToList();

        var rawPosts = dayShareIds.Count == 0
            ? new List<DaySharePostProjection>()
            : await _db.DaySharePosts
                .Where(x => dayShareIds.Contains(x.DayShareId))
                .Select(x => new DaySharePostProjection
                {
                    DayShareId = x.DayShareId,
                    PostId = x.Post.Id,
                    ImageUrl = x.Post.ImageUrl,
                    Description = x.Post.Description,
                    CreatedAt = x.Post.CreatedAt,
                    MoodEmote = x.Post.MoodEmote == null
                        ? null
                        : new EmoteDto
                        {
                            Id = x.Post.MoodEmote.Id,
                            Name = x.Post.MoodEmote.Name,
                            ImageUrl = x.Post.MoodEmote.ImageUrl
                        }
                })
                .ToListAsync();

        // 5. Group posts
        var postLookup = rawPosts
            .GroupBy(x => x.DayShareId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 6. Build items
        var items = feedEntries.Select(x => new DayShareFeedItemDto
        {
            DayShareId = x.DayShareId,
            Date = DateOnly.FromDateTime(x.SharedDate),
            Description = x.Description,
            Owner = x.Owner,
            PostCount = x.PostCount,
            CreatedAt = x.PostedAt,
            Posts = postLookup.TryGetValue(x.DayShareId, out var posts)
                ? posts.Select(p => new DaySharePostBasicDto
                {
                    Id = p.PostId,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    MoodEmote = p.MoodEmote,
                    CreatedAt = p.CreatedAt
                }).ToList()
                : new List<DaySharePostBasicDto>()
        }).ToList();

        // 7. Build cursor
        string? nextCursor = null;
        if (hasNextPage)
        {
            var last = feedEntries.Last();
            nextCursor = encodeNextCursor(last);
        }

        return new CursorPaginationResponse<DayShareFeedItemDto>(
            items, nextCursor, hasNextPage);
    }

    // Map db -> service
    private class DayShareFeedProjection
    {
        public Guid DayShareId { get; set; }
        public DateTime PostedAt { get; set; }
        public DateTime SharedDate { get; set; }
        public string? Description { get; set; }
        public DayShareOwnerDto Owner { get; set; } = null!;
        public int PostCount { get; set; }
    }

    private class DaySharePostProjection
    {
        public Guid DayShareId { get; set; }
        public Guid PostId { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public EmoteDto? MoodEmote { get; set; }
    }
}