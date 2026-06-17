using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Messages.Posts;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Post;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class PostService : IPostService
{
    private readonly AppDbContext _db;
    private readonly ICloudinaryService _cloudinaryService;
    private readonly PostPublisher _postPublisher;
    private readonly NotificationPublisher _notificationPublisher;
    private readonly IEmoteService _emoteService;
    private readonly IPostAccessService _postAccessService;
    private readonly IReactionService _reactionService;

    public PostService(
        AppDbContext db,
        ICloudinaryService cloudinaryService,
        PostPublisher postPublisher,
        NotificationPublisher notificationPublisher,
        IEmoteService emoteService,
        IPostAccessService postAccessService,
        IReactionService reactionService
    )
    {
        _db = db;
        _cloudinaryService = cloudinaryService;
        _postPublisher = postPublisher;
        _notificationPublisher = notificationPublisher;
        _emoteService = emoteService;
        _postAccessService = postAccessService;
        _reactionService = reactionService;
    }

    // CREATE POST
    public async Task<Result<PostDto>> CreateAsync(Guid userId, CreatePostRequestDto request)
    {
        // Validate mood emote
        if (!await _emoteService.HasEmoteAsync(userId, request.MoodEmoteId))
        {
            return Result<PostDto>.Fail("You do not have permission to use this mood emote");
        }

        var viewerResult = await ResolveViewerIdsAsync(
            userId,
            request.Privacy,
            request.AllowedUserIds);

        if (!viewerResult.IsSuccess)
        {
            return Result<PostDto>.Fail(viewerResult.Error!);
        }

        var viewerIds = viewerResult.Data!;

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
        await _postPublisher.PublishPostCreatedAsync(
            new PostCreatedMessage()
            {
                PostId = post.Id,
                AuthorId = userId,
                RecipientIds = viewerIds
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
        // Check valid post
        if (!await _postAccessService.IsPostOwnerAsync(postId, userId))
        {
            return Result<PostDto>.Fail("Post not found");
        }

        var post = await _db.Posts.Where(p => p.Id == postId && p.DeletedAt == null).FirstOrDefaultAsync();

        // Current viewers
        var oldViewerIds = (await _db.PostFeeds
                .Where(x => x.PostId == postId)
                .Select(x => x.ViewerId)
                .ToListAsync()
            ).ToHashSet();

        var viewerResult = await ResolveViewerIdsAsync(
            userId,
            request.Privacy,
            request.AllowedUserIds);

        if (!viewerResult.IsSuccess)
        {
            return Result<PostDto>.Fail(viewerResult.Error!);
        }

        var newViewerIds = viewerResult.Data!;

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

        await _postPublisher.PublishPostUpdatedAsync(new PostUpdatedMessage()
        {
            AuthorId = userId,
            PostId = postId,
            AddedViewerIds = addedUsers,
            RemovedViewerIds = removedUsers
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
            UpdatedAt = post.UpdatedAt
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
        await _postPublisher.PublishPostDeletedAsync(
            new PostDeletedMessage()
            {
                PostId = post.Id
            });

        return Result.Ok();
    }

    // GET POSTS OF CURRENT USER
    public async Task<Result<CursorPaginationResponse<MyPostItemDto>>> GetMyPostsAsync(Guid userId,
        GetMyPostsRequestDto request)
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
            hasNextPage,
            message: "Get feed successfully"
        );

        return Result<CursorPaginationResponse<MyPostItemDto>>.Ok(response);
    }

    // GET POST BY ID
    public async Task<Result<PostDetailDto>> GetByIdAsync(Guid currentUserId, Guid postId)
    {
        bool canView = await _postAccessService.CanViewPostAsync(postId, currentUserId);
        if (!canView)
        {
            return Result<PostDetailDto>.Fail("Can't view post");
        }

        bool isPostOwned = await _postAccessService.IsPostOwnerAsync(postId, currentUserId);

        // Get post
        var post = await _db.Posts
            .AsNoTracking()
            .Where(x =>
                x.Id == postId &&
                x.DeletedAt == null)
            .Select(x => new
            {
                OwnerId = x.UserId,

                Post = new PostDto
                {
                    Id = x.Id,
                    ImageUrl = x.ImageUrl,
                    Description = x.Description,
                    Privacy = x.Privacy,
                    CreatedAt = x.CreatedAt,
                    MoodEmote = new EmoteDto
                    {
                        Id = x.MoodEmote.Id,
                        Name = x.MoodEmote.Name,
                        ImageUrl = x.MoodEmote.ImageUrl
                    }
                },

                Owner = new UserDto
                {
                    Id = x.User.Id,
                    DisplayName = x.User.DisplayName,
                    AvatarUrl = x.User.AvatarUrl
                }
            })
            .FirstOrDefaultAsync();

        if (post == null)
        {
            return Result<PostDetailDto>.Fail("Post not found");
        }

        var response = new PostDetailDto
        {
            Post = post.Post,
            Owner = post.Owner,
            Emote = null,
            Reactions = []
        };

        if (isPostOwned)
        {
            Console.WriteLine(isPostOwned);
            var reactionsResult = await _reactionService.GetByPostAsync(
                currentUserId,
                postId);

            response.Reactions = reactionsResult.IsSuccess
                ? reactionsResult.Data!
                : [];
        }
        else
        {
            Console.WriteLine(isPostOwned);
            response.Emote = await _reactionService.GetUserReactionEmoteAsync(
                currentUserId,
                postId);
        }

        return Result<PostDetailDto>.Ok(response);
    }
    

    // GET MY POST FEED 
    public async Task<Result<CursorPaginationResponse<PostFeedItemDto>>> GetMyFeedAsync(Guid currentUserId,
        CursorPaginationRequest request)
    {
        var query = _db.PostFeeds
            .AsNoTracking()
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.Post.DeletedAt == null)
            .OrderByDescending(x => x.Post.CreatedAt)
            .ThenByDescending(x => x.Post.Id);

        var response = await GetFeedInternalAsync(
            query,
            currentUserId,
            request);

        return Result<CursorPaginationResponse<PostFeedItemDto>>.Ok(response);
    }

    // GET FRIEND POST FEED
    public async Task<Result<CursorPaginationResponse<PostFeedItemDto>>> GetFriendFeedAsync(
        Guid currentUserId, Guid profileUserId, CursorPaginationRequest request)
    {
        if (currentUserId == profileUserId)
        {
            return Result<CursorPaginationResponse<PostFeedItemDto>>.Fail("You can not access your posts");
        }

        var query = _db.PostFeeds
            .AsNoTracking()
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.Post.UserId == profileUserId &&
                x.Post.DeletedAt == null)
            .OrderByDescending(x => x.Post.CreatedAt)
            .ThenByDescending(x => x.Post.Id);

        var response = await GetFeedInternalAsync(
            query,
            currentUserId,
            request);

        return Result<CursorPaginationResponse<PostFeedItemDto>>.Ok(response);
    }

    /// <summary>
    /// Resolves and validates the list of users who can view a post based on its privacy setting.
    /// For public posts, all friends can view the post.
    /// For private posts, only the owner can view the post.
    /// For custom posts, validates selected users and friendship relationships.
    /// The post owner is always included in the viewer list.
    /// </summary>
    /// <param name="userId">The owner of the post</param>
    /// <param name="privacy">The privacy setting of the post</param>
    /// <param name="allowedUserIds">The list of users allowed to view the post when privacy is custom</param>
    /// <returns>
    /// A Result containing the final set of viewer IDs if validation succeeds;
    /// otherwise, an error message describing the validation failure.
    /// </returns>
    private async Task<Result<HashSet<Guid>>> ResolveViewerIdsAsync(
        Guid userId,
        string privacy,
        List<Guid>? allowedUserIds)
    {
        HashSet<Guid> viewerIds = [];

        switch (privacy)
        {
            case PostPrivacyConstants.Public:
                viewerIds = (await _db.Friends
                        .GetFriendIdsOfUserAsync(userId))
                    .ToHashSet();
                break;

            case PostPrivacyConstants.Private:
                break;

            case PostPrivacyConstants.Custom:
                var selectedUsers = allowedUserIds!
                    .Distinct()
                    .ToList();

                if (selectedUsers.Contains(userId))
                {
                    return Result<HashSet<Guid>>
                        .Fail("Do not include yourself in AllowedUserIds");
                }

                var existingUsers = await _db.Users
                    .CountAsync(x => selectedUsers.Contains(x.Id));

                if (existingUsers != selectedUsers.Count)
                {
                    return Result<HashSet<Guid>>
                        .Fail("Some selected users do not exist");
                }

                foreach (var viewerId in selectedUsers)
                {
                    bool areFriends =
                        await _db.Friends.AreFriendsAsync(userId, viewerId);

                    if (!areFriends)
                    {
                        return Result<HashSet<Guid>>.Fail("Some selected users are not your friends");
                    }
                }

                viewerIds = selectedUsers.ToHashSet();
                break;
        }

        viewerIds.Add(userId);

        return Result<HashSet<Guid>>.Ok(viewerIds);
    }

    /// <summary>
    /// Retrieves post feeds with cursor pagination.
    /// </summary>
    /// <param name="query">The base post feed query.</param>
    /// <param name="currentUserId">The current user identifier.</param>
    /// <param name="request">The pagination request.</param>
    /// <returns>
    /// A paginated response containing post feed items.
    /// </returns>
    private async Task<CursorPaginationResponse<PostFeedItemDto>>
        GetFeedInternalAsync(
            IQueryable<PostFeed> query,
            Guid currentUserId,
            CursorPaginationRequest request)
    {
        // Cursor pagination
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, postId) = CursorHelper.Decode(request.Cursor);

            query = query.Where(x =>
                x.Post.CreatedAt < createdAt ||
                (x.Post.CreatedAt == createdAt &&
                 x.Post.Id.CompareTo(postId) < 0));
        }

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
                    UpdatedAt = x.Post.UpdatedAt,
                    MoodEmote = new EmoteDto
                    {
                        Id = x.Post.MoodEmote.Id,
                        Name = x.Post.MoodEmote.Name,
                        ImageUrl = x.Post.MoodEmote.ImageUrl
                    }
                },

                Emote = x.Post.Reactions
                    .Where(r => r.UserId == currentUserId)
                    .Select(r => new EmoteDto
                    {
                        Id = r.Emote.Id,
                        Name = r.Emote.Name,
                        ImageUrl = r.Emote.ImageUrl
                    })
                    .FirstOrDefault(),

                Owner = new UserDto
                {
                    Id = x.Post.User.Id,
                    DisplayName = x.Post.User.DisplayName,
                    AvatarUrl = x.Post.User.AvatarUrl
                }
            })
            .Take(request.PageSize + 1)
            .ToListAsync();

        bool hasNextPage = feeds.Count > request.PageSize;

        feeds = feeds.Take(request.PageSize).ToList();

        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = feeds.Last();

            nextCursor = CursorHelper.Encode(
                lastItem.Post.CreatedAt,
                lastItem.Post.Id);
        }

        return new CursorPaginationResponse<PostFeedItemDto>(
            feeds,
            nextCursor,
            hasNextPage,
            "Get feed successfully");
    }
}