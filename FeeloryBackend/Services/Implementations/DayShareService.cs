using System.Text;
using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.DayShare;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Reaction;
using FeeloryBackend.Models.DTOs.User;
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

    public async Task<Result<DayShareDetailDto>> CreateAsync(
        Guid currentUserId,
        CreateDayShareRequestDto dto)
    {
        // 1. Validate privacy type
        if (dto.Privacy != DayShareTypeConstants.Friends &&
            dto.Privacy != DayShareTypeConstants.Custom)
            return Result<DayShareDetailDto>.Fail("Invalid privacy type.");

        // 2. Validate posts belong to user and fall on the given date
        var dayStart = DateTime.SpecifyKind(
            dto.Date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
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
            return Result<DayShareDetailDto>.Fail(
                "Some posts are invalid, do not belong to you, or are not from the given date.");

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
        else
        {
            if (dto.AllowedUserIds is null || dto.AllowedUserIds.Count == 0)
                return Result<DayShareDetailDto>.Fail(
                    "At least one allowed friend is required for Custom privacy.");

            var allowedDistinct = dto.AllowedUserIds.Distinct().ToList();

            if (allowedDistinct.Any(id => !friendIds.Contains(id)))
                return Result<DayShareDetailDto>.Fail(
                    "Some allowed users are not your friends.");

            viewerIds = allowedDistinct;
        }

        // 4. Check duplicate DayShare on the same day
        bool exists = await _db.DayShares.AnyAsync(x =>
            x.OwnerId == currentUserId &&
            x.SharedDate.Date == dto.Date.ToDateTime(TimeOnly.MinValue).Date &&
            x.DeletedAt == null);

        if (exists)
            return Result<DayShareDetailDto>.Fail("You already shared this day.");

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
        await _db.SaveChangesAsync();

        // 7. Publish feed event
        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action = DayShareFeedMessage.ActionCreated,
            DayShareId = dayShare.Id,
            ViewerIds = viewerIds
        });

        // 8. Return created DayShare
        return await GetByIdAsync(currentUserId, dayShare.Id);
    }

    public async Task<Result<DayShareDetailDto>> UpdateAsync(
        Guid currentUserId,
        UpdateDayShareRequestDto dto)
    {
        // 1. Validate privacy
        if (dto.Privacy != DayShareTypeConstants.Friends &&
            dto.Privacy != DayShareTypeConstants.Custom)
            return Result<DayShareDetailDto>.Fail("Invalid privacy type.");

        // 2. Find DayShare and verify ownership
        var dayShare = await _db.DayShares
            .FirstOrDefaultAsync(x =>
                x.Id == dto.DayShareId &&
                x.DeletedAt == null);

        if (dayShare is null)
            return Result<DayShareDetailDto>.Fail("DayShare not found.");

        if (dayShare.OwnerId != currentUserId)
            return Result<DayShareDetailDto>.Fail(
                "You are not the owner of this DayShare.");

        // 3. Validate and update posts if provided
        if (dto.SelectedPostIds is not null && dto.SelectedPostIds.Count > 0)
        {
            var dayStart = DateTime.SpecifyKind(
                dayShare.SharedDate.Date, DateTimeKind.Utc);
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
                return Result<DayShareDetailDto>.Fail(
                    "Some posts are invalid, do not belong to you, or are not from the given date.");

            var oldDaySharePosts = await _db.DaySharePosts
                .Where(x => x.DayShareId == dayShare.Id)
                .ToListAsync();

            var oldPostIds = oldDaySharePosts.Select(x => x.PostId).ToList();
            var removedPostIds = oldPostIds.Except(distinctPostIds).ToList();
            var addedPostIds = distinctPostIds.Except(oldPostIds).ToList();

            if (removedPostIds.Any())
            {
                _db.DaySharePosts.RemoveRange(
                    oldDaySharePosts.Where(x =>
                        removedPostIds.Contains(x.PostId)));
            }

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

            var allPosts = oldDaySharePosts
                .Where(x => !removedPostIds.Contains(x.PostId))
                .Concat(newDaySharePosts)
                .ToList();

            foreach (var post in allPosts)
                post.DisplayOrder = distinctPostIds.IndexOf(post.PostId) + 1;
        }

        // 4. Get old viewer list from DayShareFeeds
        var oldViewerIds = await _db.DayShareFeeds
            .Where(x => x.DayShareId == dayShare.Id)
            .Select(x => x.ViewerId)
            .ToListAsync();

        // 5. Get new viewer list
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
                return Result<DayShareDetailDto>.Fail(
                    "At least one allowed friend is required for Custom privacy.");

            var allowedDistinct = dto.AllowedUserIds.Distinct().ToList();

            if (allowedDistinct.Any(id => !friendIds.Contains(id)))
                return Result<DayShareDetailDto>.Fail(
                    "Some allowed users are not your friends.");

            newViewerIds = allowedDistinct;
        }

        // 6. Diff viewers
        var removedViewerIds = oldViewerIds.Except(newViewerIds).ToList();
        var addedViewerIds = newViewerIds.Except(oldViewerIds).ToList();

        // 7. Update DayShare fields
        dayShare.Description = dto.Description;
        dayShare.ShareType = dto.Privacy;
        dayShare.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // 8. Publish feed change events
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

        // 9. Return updated DayShare
        return await GetByIdAsync(currentUserId, dayShare.Id);
    }

    public async Task<Result<DayShareDetailDto>> GetByIdAsync(
        Guid currentUserId,
        Guid dayShareId)
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

        // 2. Check permission
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

        // 3. Fetch posts
        var rawPosts = await _db.DaySharePosts
            .Where(x => x.DayShareId == dayShareId)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new
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

        var postIds = rawPosts.Select(p => p.Id).ToList();
        List<DaySharePostItemDto> postItems;

        if (!isOwner)
        {
            // TH1: Xem DayShare của người khác → emote mình đã thả
            // var myReactions = await _db.Reactions
            //     .Where(x =>
            //         postIds.Contains(x.PostId) &&
            //         x.UserId == currentUserId)
            //     .Select(x => new
            //     {
            //         PostId = x.PostId,
            //         EmoteId = x.Emote.Id,
            //         EmoteName = x.Emote.Name,
            //         ImageUrl = x.Emote.ImageUrl
            //     })
            //     .ToListAsync();

            postItems = rawPosts.Select(p => new DaySharePostItemDto
            {
                Id = p.Id,
                ImageUrl = p.ImageUrl,
                Description = p.Description,
                MoodEmote = p.MoodEmote,
                CreatedAt = p.CreatedAt,
                // MyReaction = myReactions
                //     .Where(r => r.PostId == p.Id)
                //     .Select(r => new EmoteDto
                //     {
                //         Id = r.EmoteId,
                //         Name = r.EmoteName,
                //         ImageUrl = r.ImageUrl
                //     })
                //     .FirstOrDefault(),
                // Reactions = null
            }).ToList();
        }
        else
        {
            // TH2: Xem DayShare của mình → reactions của bạn bè
            // var friendReactions = await _db.Reactions
            //     .Where(x =>
            //         postIds.Contains(x.PostId) &&
            //         x.UserId != currentUserId)
            //     .Select(x => new
            //     {
            //         PostId = x.PostId,
            //         EmoteId = x.Emote.Id,
            //         EmoteName = x.Emote.Name,
            //         EmoteImageUrl = x.Emote.ImageUrl,
            //         UserId = x.User.Id,
            //         DisplayName = x.User.DisplayName,
            //         AvatarUrl = x.User.AvatarUrl,
            //         CreatedAt = x.CreatedAt
            //     })
            //     .ToListAsync();

            postItems = rawPosts.Select(p => new DaySharePostItemDto
            {
                Id = p.Id,
                ImageUrl = p.ImageUrl,
                Description = p.Description,
                MoodEmote = p.MoodEmote,
                CreatedAt = p.CreatedAt,
                MyReaction = null,
                // Reactions = friendReactions
                //     .Where(r => r.PostId == p.Id)
                //     .GroupBy(r => r.EmoteId)
                //     .Select(g => new ReactionGroupDto
                //     {
                //         Emote = new EmoteDto
                //         {
                //             Id = g.Key,
                //             Name = g.First().EmoteName,
                //             ImageUrl = g.First().EmoteImageUrl
                //         },
                //         Count = g.Count(),
                //         Users = g.Select(r => new ReactionUserDto
                //         {
                //             User = new UserSummaryDto
                //             {
                //                 Id = r.UserId,
                //                 DisplayName = r.DisplayName,
                //                 AvatarUrl = r.AvatarUrl
                //             },
                //             CreatedAt = r.CreatedAt
                //         }).ToList()
                //     }).ToList()
            }).ToList();
        }

        return Result<DayShareDetailDto>.Ok(new DayShareDetailDto
        {
            Id = dayShare.Id,
            Date = DateOnly.FromDateTime(dayShare.SharedDate),
            Description = dayShare.Description,
            Privacy = dayShare.ShareType,
            Owner = dayShare.Owner,
            Posts = postItems,
            CreatedAt = dayShare.SharedDate
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

        await _publisher.PublishAsync(new DayShareFeedMessage
        {
            Action = DayShareFeedMessage.ActionDeleted,
            DayShareId = dayShare.Id,
            ViewerIds = new List<Guid>()
        });

        return Result.Ok();
    }

    public async Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetFeedAsync(
        Guid currentUserId,
        CursorPaginationRequest pagination)
    {
        var pageSize = Math.Clamp(pagination.PageSize, 1, 50);

        DateTime? cursorTime = null;

        if (!string.IsNullOrEmpty(pagination.Cursor))
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(pagination.Cursor));

            if (DateTime.TryParse(decoded, null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var parsed))
            {
                cursorTime = parsed;
            }
        }

        // 1. GET FEED ENTRIES
        var feedEntries = await _db.DayShareFeeds
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.DayShare.DeletedAt == null &&
                (cursorTime == null || x.PostedAt < cursorTime))
            .OrderByDescending(x => x.PostedAt)
            .Take(pageSize + 1)
            .Select(x => new
            {
                x.DayShareId,
                x.PostedAt,
                SharedDate = x.DayShare.SharedDate,
                x.DayShare.Description,
                Owner = new DayShareOwnerDto
                {
                    Id = x.DayShare.Owner.Id,
                    DisplayName = x.DayShare.Owner.DisplayName,
                    AvatarUrl = x.DayShare.Owner.AvatarUrl
                },
                PostCount = x.DayShare.DaySharePosts.Count
            })
            .ToListAsync();

        var hasNextPage = feedEntries.Count > pageSize;
        if (hasNextPage)
            feedEntries = feedEntries.Take(pageSize).ToList();

        var dayShareIds = feedEntries.Select(x => x.DayShareId).ToList();

        // 2. GET POSTS (NO JOIN)
        var rawPosts = await _db.DaySharePosts
            .Where(x => dayShareIds.Contains(x.DayShareId))
            .Select(x => new
            {
                x.DayShareId,
                PostId = x.Post.Id,
                x.Post.ImageUrl,
                x.Post.Description,
                x.Post.CreatedAt,
                MoodEmote = x.Post.MoodEmote == null
                    ? null
                    : new DayShareMoodEmoteDto
                    {
                        Id = x.Post.MoodEmote.Id,
                        Name = x.Post.MoodEmote.Name,
                        ImageUrl = x.Post.MoodEmote.ImageUrl
                    }
            })
            .ToListAsync();

        // 3. GROUP POSTS (FIX PERFORMANCE)
        var postLookup = rawPosts
            .GroupBy(x => x.DayShareId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var postIds = rawPosts.Select(x => x.PostId).ToList();

        // 4. REACTIONS (SIMPLE - POST + USER)
        // var myReactions = await _db.Reactions
        //     .Where(x =>
        //         postIds.Contains(x.PostId) &&
        //         x.UserId == currentUserId)
        //     .Select(x => new
        //     {
        //         x.PostId,
        //         EmoteId = x.Emote.Id,
        //         EmoteName = x.Emote.Name,
        //         ImageUrl = x.Emote.ImageUrl
        //     })
        //     .ToListAsync();
        //
        // var reactionLookup = myReactions
        //     .GroupBy(x => x.PostId)
        //     .ToDictionary(g => g.Key, g => g.First());

        // 5. BUILD RESULT
        var items = feedEntries.Select(x => new DayShareFeedItemDto
        {
            DayShareId = x.DayShareId,
            Date = DateOnly.FromDateTime(x.SharedDate),
            Description = x.Description,
            Owner = x.Owner,
            PostCount = x.PostCount,
            CreatedAt = x.PostedAt,

            Posts = postLookup.ContainsKey(x.DayShareId)
                ? postLookup[x.DayShareId].Select(p => new DaySharePostItemDto
                {
                    Id = p.PostId,
                    ImageUrl = p.ImageUrl,
                    Description = p.Description,
                    MoodEmote = p.MoodEmote,
                    CreatedAt = p.CreatedAt,

                    // MyReaction = reactionLookup.ContainsKey(p.PostId)
                    //     ? new EmoteDto
                    //     {
                    //         Id = reactionLookup[p.PostId].EmoteId,
                    //         Name = reactionLookup[p.PostId].EmoteName,
                    //         ImageUrl = reactionLookup[p.PostId].ImageUrl
                    //     }
                    //     : null,

                    // Reactions = null
                }).ToList()
                : new List<DaySharePostItemDto>()
        }).ToList();

        // 6. CURSOR (STABLE)
        string? nextCursor = null;

        if (hasNextPage)
        {
            var last = feedEntries.Last();
            var cursor = $"{last.PostedAt:o}|{last.DayShareId}";
            nextCursor = Convert.ToBase64String(Encoding.UTF8.GetBytes(cursor));
        }

        return Result<CursorPaginationResponse<DayShareFeedItemDto>>.Ok(
            new CursorPaginationResponse<DayShareFeedItemDto>(
                items,
                nextCursor,
                hasNextPage));
    }

    public async Task<Result<CursorPaginationResponse<DayShareFeedItemDto>>> GetUserFeedAsync(
        Guid currentUserId,
        Guid targetUserId,
        CursorPaginationRequest pagination)
    {
        var pageSize = Math.Clamp(pagination.PageSize, 1, 50);

        DateTime? cursorTime = null;
        if (!string.IsNullOrEmpty(pagination.Cursor))
        {
            var decoded = Encoding.UTF8.GetString(
                Convert.FromBase64String(pagination.Cursor));
            if (DateTime.TryParse(decoded, null,
                    System.Globalization.DateTimeStyles.RoundtripKind,
                    out var parsed))
                cursorTime = parsed;
        }

        bool isOwner = currentUserId == targetUserId;

        var dayShares = await _db.DayShares
            .Where(x =>
                x.OwnerId == targetUserId &&
                x.DeletedAt == null &&
                (isOwner || x.DayShareFeeds.Any(f => f.ViewerId == currentUserId)) &&
                (cursorTime == null || x.SharedDate < cursorTime))
            .OrderByDescending(x => x.SharedDate)
            .Take(pageSize + 1)
            .Select(x => new
            {
                x.Id,
                x.SharedDate,
                x.Description,
                OwnerId = x.Owner.Id,
                DisplayName = x.Owner.DisplayName,
                AvatarUrl = x.Owner.AvatarUrl,
                PostCount = x.DaySharePosts.Count
            })
            .ToListAsync();

        var hasNextPage = dayShares.Count > pageSize;
        if (hasNextPage) dayShares = dayShares.Take(pageSize).ToList();

        var dayShareIds = dayShares.Select(x => x.Id).ToList();

        var rawPosts = await _db.DaySharePosts
            .Where(x => dayShareIds.Contains(x.DayShareId))
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new
            {
                x.DayShareId,
                PostId = x.Post.Id,
                ImageUrl = x.Post.ImageUrl,
                MoodEmote = x.Post.MoodEmote == null
                    ? null
                    : new DayShareMoodEmoteDto
                    {
                        Id = x.Post.MoodEmote.Id,
                        Name = x.Post.MoodEmote.Name,
                        ImageUrl = x.Post.MoodEmote.ImageUrl
                    }
            })
            .ToListAsync();

        var postIds = rawPosts.Select(x => x.PostId).ToList();

        // TH1: xem feed của người khác → emote mình đã thả
        // var myReactions = !isOwner
        //     ? await _db.Reactions
        //         .Where(x =>
        //             postIds.Contains(x.PostId) &&
        //             x.UserId == currentUserId)
        //         .Select(x => new
        //         {
        //             PostId = x.PostId,
        //             EmoteId = x.Emote.Id,
        //             EmoteName = x.Emote.Name,
        //             ImageUrl = x.Emote.ImageUrl
        //         })
        //         .ToListAsync()
        //     : null;

        // TH2: xem feed của mình → reactions của bạn bè
        // var friendReactions = isOwner
        //     ? await _db.Reactions
        //         .Where(x =>
        //             postIds.Contains(x.PostId) &&
        //             x.UserId != currentUserId)
        //         .Select(x => new
        //         {
        //             PostId = x.PostId,
        //             EmoteId = x.Emote.Id,
        //             EmoteName = x.Emote.Name,
        //             EmoteImageUrl = x.Emote.ImageUrl,
        //             UserId = x.User.Id,
        //             DisplayName = x.User.DisplayName,
        //             AvatarUrl = x.User.AvatarUrl,
        //             CreatedAt = x.CreatedAt
        //         })
        //         .ToListAsync()
            // : null;

        var items = dayShares.Select(x => new DayShareFeedItemDto
        {
            DayShareId = x.Id,
            Date = DateOnly.FromDateTime(x.SharedDate),
            Description = x.Description,
            Owner = new DayShareOwnerDto
            {
                Id = x.OwnerId,
                DisplayName = x.DisplayName,
                AvatarUrl = x.AvatarUrl
            },
            PostCount = x.PostCount,
            CreatedAt = x.SharedDate,
            Posts = rawPosts
                .Where(p => p.DayShareId == x.Id)
                .Select(p => new DaySharePostItemDto
                {
                    Id = p.PostId,
                    ImageUrl = p.ImageUrl,
                    MoodEmote = p.MoodEmote,
                    // MyReaction = !isOwner
                    //     ? myReactions!
                    //         .Where(r => r.PostId == p.PostId)
                    //         .Select(r => new EmoteDto
                    //         {
                    //             Id = r.EmoteId,
                    //             Name = r.EmoteName,
                    //             ImageUrl = r.ImageUrl
                    //         })
                    //         .FirstOrDefault()
                    //     : null,
                    // Reactions = isOwner
                    //     ? friendReactions!
                    //         .Where(r => r.PostId == p.PostId)
                    //         .GroupBy(r => r.EmoteId)
                    //         .Select(g => new ReactionGroupDto
                    //         {
                    //             Emote = new EmoteDto
                    //             {
                    //                 Id = g.Key,
                    //                 Name = g.First().EmoteName,
                    //                 ImageUrl = g.First().EmoteImageUrl
                    //             },
                    //             Count = g.Count(),
                    //             Users = g.Select(r => new ReactionUserDto
                    //             {
                    //                 User = new UserSummaryDto
                    //                 {
                    //                     Id = r.UserId,
                    //                     DisplayName = r.DisplayName,
                    //                     AvatarUrl = r.AvatarUrl
                    //                 },
                    //                 CreatedAt = r.CreatedAt
                    //             }).ToList()
                    //         }).ToList()
                    //     : null
                }).ToList()
        }).ToList();

        string? nextCursor = null;
        if (hasNextPage)
        {
            var cursorStr = items.Last().CreatedAt.ToString("O");
            nextCursor = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(cursorStr));
        }

        return Result<CursorPaginationResponse<DayShareFeedItemDto>>.Ok(
            new CursorPaginationResponse<DayShareFeedItemDto>(
                items, nextCursor, hasNextPage));
    }
}