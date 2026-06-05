using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Messaging.RabbitMQ.Queues;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Post;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace FeeloryBackend.Services.Implementations;

public class PostService : IPostService
{
    private readonly AppDbContext _db;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly PostPublisher _postPublisher;

    public PostService(AppDbContext db, ICloudinaryService cloudinaryService, PostPublisher postPublisher)
    {
        _db = db;
        _cloudinaryService = cloudinaryService;
        _postPublisher = postPublisher;
    }

    // CREATE POST
    public async Task<Result<PostDto>> CreateAsync(Guid userId, CreatePostRequestDto request)
    {
        // Validate mood emote
        // bool canUseEmote = await _db.CanUseEmoteAsync(userId, request.MoodEmoteId);
        //
        // if (!canUseEmote)
        //     return Result<PostDto>.Fail("You do not have permission to use this mood emote");

        // Privacy logic
        List<Guid> viewerIds = [];

        switch (request.Privacy)
        {
            case PostPrivacyConstants.Public:
                viewerIds = await _db.Friends
                    .Where(x => x.UserId == userId)
                    .Select(x => x.FriendId)
                    .ToListAsync();
                break;

            case PostPrivacyConstants.Private:
                break;

            case PostPrivacyConstants.Custom:
                var selectedUsers = request.AllowedUserIds!
                    .Distinct()
                    .ToList();

                if (selectedUsers.Contains(userId))
                    return Result<PostDto>.Fail("Do not include yourself in AllowedUserIds");

                var existingUsers = await _db.Users.CountAsync(x => selectedUsers.Contains(x.Id));

                if (existingUsers != selectedUsers.Count)
                    return Result<PostDto>.Fail("Some selected users do not exist");

                foreach (var viewerId in selectedUsers)
                {
                    bool areFriends = await _db.Friends.AreFriendsAsync(userId, viewerId);

                    if (!areFriends)
                        return Result<PostDto>.Fail("Some selected users are not your friends");
                }

                viewerIds = selectedUsers;
                break;
        }

        // Add owner
        viewerIds.Add(userId);

        // Upload image
        var imageUrl = await _cloudinaryService.UploadImageAsync(request.Image);

        // Create post
        var post = new Post
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            MoodEmoteId = request.MoodEmoteId,
            ImageUrl = imageUrl,
            Description = request.Description,
            Privacy = request.Privacy,
            CreatedAt = DateTime.UtcNow
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // Publish RabbitMQ
        await _postPublisher.PublishPostAsync(
            new PostMessage
            {
                Action = PostMessage.ActionCreated,
                PostId = post.Id,
                ViewerIds = viewerIds
            });

        await _db.Entry(post).Reference(x => x.User).LoadAsync();
        await _db.Entry(post).Reference(x => x.MoodEmote).LoadAsync();

        var response = new PostDto
        {
            Id = post.Id,
            ImageUrl = post.ImageUrl,
            Description = post.Description,
            Privacy = post.Privacy,
            MoodEmote = new EmoteDto
            {
                Id = post.MoodEmote.Id,
                Name = post.MoodEmote.Name,
                ImageUrl = post.MoodEmote.ImageUrl
            },
            CreatedAt = post.CreatedAt,
        };

        return Result<PostDto>.Ok(response);
    }

    // UPDATE POST
    public async Task<Result<PostDto>> UpdateAsync(Guid userId, Guid postId, UpdatePostRequestDto request)
    {
        // Find post
        var post = await _db.Posts
            .FirstOrDefaultAsync(x => x.Id == postId && x.UserId == userId);

        if (post == null)
            return Result<PostDto>.Fail("Post not found");

        // Current viewers
        var oldViewerIds = (await _db.PostFeeds
                .Where(x => x.PostId == postId)
                .Select(x => x.ViewerId)
                .ToListAsync()
            ).ToHashSet();

        // New viewers
        HashSet<Guid> newViewerIds = [];

        switch (request.Privacy)
        {
            case PostPrivacyConstants.Public:
                var friendIds = await _db.Friends
                    .Where(x => x.UserId == userId)
                    .Select(x => x.FriendId)
                    .ToListAsync();
                newViewerIds = friendIds.ToHashSet();
                break;

            case PostPrivacyConstants.Private:
                break;

            case PostPrivacyConstants.Custom:
                var selectedUsers = request.AllowedUserIds!
                    .Distinct()
                    .ToList();

                if (selectedUsers.Contains(userId))
                    return Result<PostDto>.Fail("Do not include yourself in AllowedUserIds");

                // Check user exists
                var existingUsers = await _db.Users
                    .CountAsync(x => selectedUsers.Contains(x.Id));

                if (existingUsers != selectedUsers.Count)
                    return Result<PostDto>.Fail("Some selected users do not exist");

                // Check friendship
                foreach (var viewerId in selectedUsers)
                {
                    bool areFriends = await _db.Friends.AreFriendsAsync(userId, viewerId);

                    if (!areFriends)
                        return Result<PostDto>.Fail("Some selected users are not your friends");
                }

                newViewerIds = selectedUsers.ToHashSet();
                break;
        }

        // Add owner
        newViewerIds.Add(userId);

        // Calculate diff
        var removedUsers = oldViewerIds.Except(newViewerIds).ToList();
        var addedUsers = newViewerIds.Except(oldViewerIds).ToList();

        // Update post
        post.Description = request.Description.Trim();
        post.Privacy = request.Privacy;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Publish RabbitMQ
        if (addedUsers.Any())
        {
            await _postPublisher.PublishPostAsync(
                new PostMessage
                {
                    Action = PostMessage.ActionAdded,
                    PostId = post.Id,
                    ViewerIds = addedUsers
                });
        }

        if (removedUsers.Any())
        {
            await _postPublisher.PublishPostAsync(
                new PostMessage
                {
                    Action = PostMessage.ActionRemoved,
                    PostId = post.Id,
                    ViewerIds = removedUsers
                });
        }

        await _db.Entry(post).Reference(x => x.User).LoadAsync();
        await _db.Entry(post).Reference(x => x.MoodEmote).LoadAsync();

        var response = new PostDto
        {
            Id = post.Id,
            ImageUrl = post.ImageUrl,
            Description = post.Description,
            Privacy = post.Privacy,
            MoodEmote = new EmoteDto
            {
                Id = post.MoodEmote.Id,
                Name = post.MoodEmote.Name,
                ImageUrl = post.MoodEmote.ImageUrl
            },
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
        };
        return Result<PostDto>.Ok(response);
    }

    // DELETE POST
    public async Task<Result> DeleteAsync(Guid userId, Guid postId)
    {
        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId && p.DeletedAt == null);

        if (post == null)
            return Result.Fail("Post not found");

        // Soft delete
        post.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Publish RabbitMQ
        await _postPublisher.PublishPostAsync(
            new PostMessage
            {
                Action = PostMessage.ActionDeleted,
                PostId = post.Id
            });

        return Result.Ok();
    }

    // GET POSTS OF CURRENT USER
    public async Task<Result<CursorPaginationResponse<MyPostItemDto>>> GetMyPostsAsync(Guid userId, GetMyPostsRequestDto request)
    {
        var query = _db.Posts.AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null);

        // Filter date
        if (request.Date.HasValue)
        {
            var startDate = request.Date.Value.Date;
            var endDate = startDate.AddDays(1);

            query = query.Where(x =>
                x.CreatedAt >= startDate &&
                x.CreatedAt < endDate);
        }

        // Filter privacy
        if (!string.IsNullOrWhiteSpace(request.Privacy))
        {
            query = query.Where(x =>
                x.Privacy == request.Privacy.Trim());
        }

        // Total records after filtering
        var total = await query.CountAsync();

        // Cursor pagination
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) = CursorHelper.Decode(request.Cursor);

            query = query.Where(x =>
                x.CreatedAt < createdAt ||
                (x.CreatedAt == createdAt &&
                 x.Id.CompareTo(id) < 0));
        }

        // Sort
        query = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        // Take limit + 1
        var posts = await query
            .Take(request.PageSize + 1)
            .Select(x => new MyPostItemDto
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl,
                Description = x.Description,
                Privacy = x.Privacy,
                MoodEmote = x.MoodEmote.Name,
                ReactionCount = x.Reactions.Count,
                CreatedAt = x.CreatedAt
            }).ToListAsync();

        bool hasNextPage = posts.Count > request.PageSize;

        posts = posts.Take(request.PageSize).ToList();

        // Next cursor
        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = posts.Last();
            nextCursor = CursorHelper.Encode(lastItem.CreatedAt, lastItem.Id);
        }

        var response = new CursorPaginationResponse<MyPostItemDto>(
            posts,
            nextCursor,
            hasNextPage
        );

        return Result<CursorPaginationResponse<MyPostItemDto>>.Ok(response);
    }

    // GET POST BY ID
    public async Task<Result<PostDetailDto>> GetByIdAsync(Guid currentUserId, Guid postId)
    {
        // Check permission
        var postFeedAccess = await _db.PostFeeds
            .AsNoTracking()
            .AnyAsync(x =>
                x.PostId == postId &&
                x.ViewerId == currentUserId);
        
        // Get post
        var post = await _db.Posts
            .AsNoTracking()
            .Where(x =>
                x.Id == postId &&
                x.DeletedAt == null)
            .Select(x => new
            {
                x.UserId,
                Post = new PostDetailDto
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    Description = x.Description,
                    Privacy = x.Privacy,
                    MoodEmote = x.MoodEmote.Name,
                    CreatedAt = x.CreatedAt,
                    Owner = new UserDto
                    {
                        Id = x.User.Id,
                        DisplayName = x.User.DisplayName,
                        AvatarUrl = x.User.AvatarUrl
                    },
                    Reactions = x.Reactions
                        .Select(r => new PostReactionDto
                        {
                            UserId = r.UserId,
                            DisplayName = r.User.DisplayName,
                            ReactionName = r.Emote.Name,
                            Icon = r.Emote.ImageUrl
                        }).ToList()
                }
            }).FirstOrDefaultAsync();

        if (post == null)
            return Result<PostDetailDto>.Fail("Post not found");

        var isOwner = post.UserId == currentUserId;

        if (!isOwner && !postFeedAccess)
            return Result<PostDetailDto>.Fail("You do not have permission to get this post");
        
        return Result<PostDetailDto>.Ok(post.Post);
    }

    // GET MY POST FEED 
    public async Task<Result<CursorPaginationResponse<PostFeedItemDto>>> GetMyFeedAsync(Guid currentUserId, CursorPaginationRequest request)
    {
        var query = _db.PostFeeds
            .AsNoTracking()
            .Where(x => x.ViewerId == currentUserId && x.Post.DeletedAt == null)
            .OrderByDescending(x => x.Post.CreatedAt)
            .ThenByDescending(x => x.Post.Id)
            .AsQueryable();

        // Cursor pagination
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) = CursorHelper.Decode(request.Cursor);

            query = query.Where(x =>
                x.Post.CreatedAt < createdAt ||
                (x.Post.CreatedAt == createdAt &&
                 x.Post.Id.CompareTo(id) < 0));
        }
        
        // Get extra item
        var feeds = await query
            .Select(x => new PostFeedItemDto
            {
                Post = new PostDto
                {
                    Id = x.Post.Id,
                    ImageUrl = x.Post.ImageUrl,
                    Description = x.Post.Description,
                    Privacy = x.Post.Privacy,
                    CreatedAt = x.Post.CreatedAt,
                    MoodEmote = new EmoteDto
                        {
                            Id = x.Post.MoodEmote.Id,
                            Name = x.Post.MoodEmote.Name,
                            ImageUrl = x.Post.MoodEmote.ImageUrl
                        }
                },
                Owner = new UserDto
                {
                    Id = x.Post.User.Id,
                    DisplayName = x.Post.User.DisplayName,
                    AvatarUrl = x.Post.User.AvatarUrl
                }
            }).Take(request.PageSize + 1).ToListAsync();
        
        bool hasNextPage = feeds.Count > request.PageSize;
        feeds = feeds.Take(request.PageSize).ToList();

        // Next cursor
        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = feeds.Last();

            nextCursor = CursorHelper.Encode(
                lastItem.Post.CreatedAt,
                lastItem.Post.Id
            );
        }

        var response = new CursorPaginationResponse<PostFeedItemDto>(
                feeds,
                nextCursor,
                hasNextPage
            );

        return Result<CursorPaginationResponse<PostFeedItemDto>>.Ok(response);
    }

    // GET FRIEND POST FEED
    public async Task<Result<CursorPaginationResponse<PostFeedItemDto>>> GetFriendFeedAsync(
        Guid currentUserId, Guid profileUserId, CursorPaginationRequest request)
    {
        // Check if current user access owner posts
        if (currentUserId == profileUserId)
            return Result<CursorPaginationResponse<PostFeedItemDto>>.Fail("You can not access your posts");

        var query = _db.PostFeeds
            .AsNoTracking()
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.Post.UserId == profileUserId &&
                x.Post.DeletedAt == null)
            .OrderByDescending(x => x.Post.CreatedAt)
            .ThenByDescending(x => x.Post.Id)
            .AsQueryable();

        // Cursor pagination
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, postId) = CursorHelper.Decode(request.Cursor);

            query = query.Where(x =>
                x.Post.CreatedAt < createdAt ||
                (x.Post.CreatedAt == createdAt &&
                 x.Post.Id.CompareTo(postId) < 0));
        }

        // Get extra item
        var feeds = await query
            .Select(x => new PostFeedItemDto
            {
                Post = new PostDto
                {
                    Id = x.Post.Id,
                    ImageUrl = x.Post.ImageUrl,
                    Description = x.Post.Description,
                    Privacy = x.Post.Privacy,
                    CreatedAt = x.Post.CreatedAt,
                    MoodEmote = new EmoteDto
                        {
                            Id = x.Post.MoodEmote.Id,
                            Name = x.Post.MoodEmote.Name,
                            ImageUrl = x.Post.MoodEmote.ImageUrl
                        }
                },
                Owner = new UserDto
                {
                    Id = x.Post.User.Id,
                    DisplayName = x.Post.User.DisplayName,
                    AvatarUrl = x.Post.User.AvatarUrl
                }
            }).Take(request.PageSize + 1).ToListAsync();
        
        bool hasNextPage = feeds.Count > request.PageSize;
        feeds = feeds.Take(request.PageSize).ToList();

        // Next cursor
        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = feeds.Last();

            nextCursor = CursorHelper.Encode(
                lastItem.Post.CreatedAt,
                lastItem.Post.Id
            );
        }

        var response = new CursorPaginationResponse<PostFeedItemDto>(
                feeds,
                nextCursor,
                hasNextPage
            );

        return Result<CursorPaginationResponse<PostFeedItemDto>>.Ok(response);
    }
    
    /// <summary>
    /// Retrieves a post by its identifier.
    /// Returns null if the post does not exist or has been deleted.
    /// </summary>
    /// <param name="postId">
    /// The unique identifier of the post.
    /// </param>
    /// <returns>
    /// A <see cref="PostDetailDto"/> containing the post information;
    /// otherwise, null if the post is not found.
    /// </returns>
    public async Task<PostDetailDto?> FindByIdAsync(Guid postId)
    {
        return await _db.Posts
            .AsNoTracking()
            .Where(x =>
                x.Id == postId &&
                x.DeletedAt == null)
            .Select(x => new PostDetailDto
            {
                Id = x.Id,
                ImageUrl = x.ImageUrl,
                Description = x.Description,
                Privacy = x.Privacy,
                MoodEmote = x.MoodEmote.Name,
                CreatedAt = x.CreatedAt,
                Owner = new UserDto
                {
                    Id = x.User.Id,
                    DisplayName = x.User.DisplayName,
                    AvatarUrl = x.User.AvatarUrl
                },
                Reactions = x.Reactions
                    .Select(r => new PostReactionDto
                    {
                        UserId = r.UserId,
                        DisplayName = r.User.DisplayName,
                        ReactionName = r.Emote.Name,
                        Icon = r.Emote.ImageUrl
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}