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

    // Create 
    public async Task<Result> CreateAsync(Guid currentUserId, CreateDayShareRequestDto dto)
    {
        // 1. Validate privacy type
        if (dto.Privacy != DayShareTypeConstants.Friends &&
            dto.Privacy != DayShareTypeConstants.Custom)
        {
            return Result.Fail("Invalid privacy type.");
        }
// Validate selected posts
        if (dto.SelectedPostIds == null || !dto.SelectedPostIds.Any())
        {
            return Result.Fail("At least one post is required.");
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
// Check duplicate DayShare in same day
        bool exists = await _db.DayShares.AnyAsync(x =>
            x.OwnerId == currentUserId &&
            x.SharedDate.Date == dto.Date.ToDateTime(TimeOnly.MinValue).Date &&
            x.DeletedAt == null);

        if (exists)
        {
            return Result.Fail("You already shared this day.");
        }
        // 4. Persist DayShare
        var dayShare = new DayShare
        {
            Id          = Guid.NewGuid(),
            OwnerId     = currentUserId,
            Description = dto.Description,
            SharedDate = DateTime.SpecifyKind(
                dto.Date.ToDateTime(TimeOnly.MinValue), 
                DateTimeKind.Utc),
            ShareType   = dto.Privacy
        };

        _db.DayShares.Add(dayShare);

        // 5. Persist ordered post references
        var now = DateTime.UtcNow;
        var daySharePosts = dto.SelectedPostIds
            .Select((postId, index) => new DaySharePost
            {
                Id           = Guid.NewGuid(),
                DayShareId   = dayShare.Id,
                PostId       = postId,
                DisplayOrder = index + 1,
                CreatedAt    = now        // thêm dòng này
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
            Action = DayShareFeedMessage.ActionCreated,
            DayShareId = dayShare.Id,
            ViewerIds  = viewerIds
        });

        return Result.Ok();
    }
    
    // UPDATE
public async Task<Result> UpdateAsync(
    Guid currentUserId,
    UpdateDayShareRequestDto dto)
{
    // 1. Validate privacy
    if (dto.Privacy != DayShareTypeConstants.Friends &&
        dto.Privacy != DayShareTypeConstants.Custom)
        return Result.Fail("Invalid privacy type.");

    // 2. Tìm DayShare, kiểm tra owner
    var dayShare = await _db.DayShares
        .FirstOrDefaultAsync(x =>
            x.Id == dto.DayShareId &&
            x.DeletedAt == null);

    if (dayShare is null)
        return Result.Fail("DayShare not found.");

    if (dayShare.OwnerId != currentUserId)
        return Result.Fail("You are not the owner of this DayShare.");

    // 3. Lấy viewer cũ dựa trên ShareType hiện tại
    var oldViewerIds = await _db.DayShareFeeds
        .Where(x => x.DayShareId == dayShare.Id)
        .Select(x => x.ViewerId)
        .ToListAsync();

    // 4. Lấy viewer mới dựa trên privacy mới
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
            return Result.Fail("At least one allowed friend is required for Custom privacy.");

        var allowedDistinct = dto.AllowedUserIds.Distinct().ToList();

        if (allowedDistinct.Any(id => !friendIds.Contains(id)))
            return Result.Fail("Some allowed users are not your friends.");

        newViewerIds = allowedDistinct;
    }

    // 5. Diff viewer cũ vs mới
    var removedViewerIds = oldViewerIds.Except(newViewerIds).ToList();
    var addedViewerIds   = newViewerIds.Except(oldViewerIds).ToList();

    // 6. Cập nhật DayShare
    dayShare.Description = dto.Description;
    dayShare.ShareType   = dto.Privacy;
    dayShare.UpdatedAt   = DateTime.UtcNow;

    // 7. Cập nhật DayShareViewers
    var oldViewers = await _db.DayShareViewers
        .Where(x => x.DayShareId == dayShare.Id)
        .ToListAsync();

    _db.DayShareViewers.RemoveRange(oldViewers);

    if (dto.Privacy == DayShareTypeConstants.Custom)
    {
        _db.DayShareViewers.AddRange(newViewerIds.Select(viewerId =>
            new DayShareViewer
            {
                Id         = Guid.NewGuid(),
                DayShareId = dayShare.Id,
                ViewerId   = viewerId
            }));
    }

    await _db.SaveChangesAsync();

    // 8. Publish event thay đổi feed qua RabbitMQ
    if (removedViewerIds.Any())
    {
        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action     = DayShareFeedMessage.ActionRemoved,
            DayShareId = dayShare.Id,
            ViewerIds  = removedViewerIds
        });
    }

    if (addedViewerIds.Any())
    {
        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action     = DayShareFeedMessage.ActionAdded,
            DayShareId = dayShare.Id,
            ViewerIds  = addedViewerIds
        });
    }

    return Result.Ok();
}

// getbyid
public async Task<Result<DayShareDetailDto>> GetByIdAsync(
    Guid currentUserId,
    Guid dayShareId)
{
    // 1. Lấy DayShare
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
                Id          = x.Owner.Id,
                DisplayName = x.Owner.DisplayName,
                AvatarUrl   = x.Owner.AvatarUrl
            }
        })
        .FirstOrDefaultAsync();

    if (dayShare is null)
        return Result<DayShareDetailDto>.Fail("DayShare not found.");

    // 2. Kiểm tra quyền xem: là owner HOẶC có trong feed
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

    // 3. Lấy danh sách posts theo DisplayOrder
    var posts = await _db.DaySharePosts
        .Where(x => x.DayShareId == dayShareId)
        .OrderBy(x => x.DisplayOrder)
        .Select(x => new DaySharePostItemDto
        {
            Id          = x.Post.Id,
            ImageUrl    = x.Post.ImageUrl,
            Description = x.Post.Description,
            MoodEmote   = x.Post.MoodEmote == null
                ? null
                : new DayShareMoodEmoteDto
                {
                    Id       = x.Post.MoodEmote.Id,
                    Name     = x.Post.MoodEmote.Name,
                    ImageUrl = x.Post.MoodEmote.ImageUrl
                },
            CreatedAt = x.Post.CreatedAt
        })
        .ToListAsync();

    return Result<DayShareDetailDto>.Ok(new DayShareDetailDto
    {
        Id          = dayShare.Id,
        Date        = DateOnly.FromDateTime(dayShare.SharedDate),
        Description = dayShare.Description,
        Privacy     = dayShare.ShareType,
        Owner       = dayShare.Owner,
        Posts       = posts,
       
    });
}

// delete
    public async Task<Result> DeleteAsync(
        Guid currentUserId,
        Guid dayShareId)
    {
        // 1. Tìm DayShare, kiểm tra owner
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

        // 3. Publish để xóa feed của tất cả bạn bè đã nhận
        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action     = DayShareFeedMessage.ActionDeleted,
            DayShareId = dayShare.Id,
          //  ViewerIds  = new List<Guid>()
        });

        return Result.Ok();
    }
    
   public async Task<Result<DayShareFeedPagedDto>> GetFeedAsync(
    Guid currentUserId,
    int page,
    int pageSize)
{
    try
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        var query = _db.DayShareFeeds
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.DayShare.DeletedAt == null)
            .OrderByDescending(x => x.PostedAt);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DayShareFeedItemDto
            {
                DayShareId = x.DayShareId,
                Date = DateOnly.FromDateTime(x.DayShare.SharedDate),
                Description = x.DayShare.Description,

                Owner = new DayShareOwnerDto
                {
                    Id = x.DayShare.OwnerId,
                    DisplayName = x.DayShare.Owner.DisplayName ?? "",
                    AvatarUrl = x.DayShare.Owner.AvatarUrl
                },

                PostCount = x.DayShare.DaySharePosts.Count, 

                CreatedAt = x.PostedAt
            })
            .ToListAsync();

        return Result<DayShareFeedPagedDto>.Ok(
            new DayShareFeedPagedDto
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize
            });
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString()); // hoặc dùng ILogger
        return Result<DayShareFeedPagedDto>.Fail("An error occurred while loading feed.");
    }
}
}