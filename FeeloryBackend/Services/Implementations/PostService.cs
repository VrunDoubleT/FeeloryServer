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
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Post;
using FeeloryBackend.Models.Entities;
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
        bool canUseEmote = await _db.CanUseEmoteAsync(userId, request.MoodEmoteId);

        if (!canUseEmote)
            return Result<PostDto>.Fail("You do not have permission to use this mood emote");

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
            User = new UserDto
            {
                Id = post.User.Id,
                Username = post.User.Username,
                AvatarUrl = post.User.AvatarUrl
            },
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
        
        Console.WriteLine($"[UPDATE POST] PostId: {postId}");
        Console.WriteLine($"Old viewers: {oldViewerIds.Count}");
        Console.WriteLine($"New viewers: {newViewerIds.Count}");
        Console.WriteLine($"Added: {addedUsers.Count}");
        Console.WriteLine($"Removed: {removedUsers.Count}");
        Console.WriteLine($"RemovedIds: {string.Join(",", removedUsers)}");

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
            Console.WriteLine($"[PUBLISH REMOVE] PostId: {post.Id}");
            Console.WriteLine($"Removing viewers: {string.Join(",", removedUsers)}");
            
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
            User = new UserDto
            {
                Id = post.User.Id,
                Username = post.User.Username,
                AvatarUrl = post.User.AvatarUrl
            },
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

    // GET POSTS BY USER
    public async Task<GetMyPostsResponseDto> GetMyPostsAsync(Guid userId, GetMyPostsRequestDto request)
    {
        var query = _db.Posts.AsNoTracking()
            .Where(x => x.UserId == userId && x.DeletedAt == null);

        // Filter date
        if (request.Date.HasValue)
        {
            var startDate = request.Date.Value.Date;
            var endDate = startDate.AddDays(1);

            query = query.Where(x => x.CreatedAt >= startDate && x.CreatedAt < endDate);
        }

        // Filter privacy
        if (!string.IsNullOrWhiteSpace(request.Privacy))
            query = query.Where(x => x.Privacy == request.Privacy.Trim());
        
        // Total records after filtering
        var total = await query.CountAsync();
        
        // Cursor pagination
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) = CursorHelper.Decode(request.Cursor);
            query = query.Where(x =>
                x.CreatedAt < createdAt ||
                (x.CreatedAt == createdAt && x.Id != id));
        }

        // Sort
        query = query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id);

        // Take limit + 1
        var posts = await query
            .Take(request.Limit + 1)
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

        // Next cursor
        string? nextCursor = null;

        if (posts.Count > request.Limit)
        {
            var lastVisibleItem = posts[request.Limit - 1];
            nextCursor = CursorHelper.Encode(lastVisibleItem.CreatedAt, lastVisibleItem.Id);
            posts.RemoveAt(request.Limit);
        }

        // Return post
        return new GetMyPostsResponseDto
        {
            Items = posts,
            Total = total,
            NextCursor = nextCursor
        };
    }
    
    // GET POST BY ID
    public async Task<Result<PostDetailDto>> GetByIdAsync(Guid currentUserId, Guid postId)
    {
        // Get post
        var post = await _db.Posts.AsNoTracking()
            .Where(x => x.Id == postId && x.DeletedAt == null)
            .Select(x => new
            {
                x.Id,
                x.ImageUrl,
                x.Description,
                x.Privacy,
                MoodEmote = x.MoodEmote != null ? x.MoodEmote.Name : null,
                x.CreatedAt,
                x.UserId,
                Owner = new UserDto
                {
                    Id = x.User.Id,
                    DisplayName = x.User.DisplayName,
                    AvatarUrl = x.User.AvatarUrl
                },
                Reactions = x.Reactions.Select(r => new PostReactionDto
                {
                    UserId = r.UserId,
                    DisplayName = r.User.DisplayName,
                    Reaction = r.Emote != null ? r.Emote.Name : null
                }).ToList()
            }).FirstOrDefaultAsync();

        if (post == null)
            return Result<PostDetailDto>.Fail("Post not found");

        // Check permission (owner OR in feed)
        var isOwner = post.UserId == currentUserId;

        var inFeed = await _db.PostFeeds
            .AnyAsync(x => x.PostId == postId && x.ViewerId == currentUserId);

        if (!isOwner && !inFeed)
            return Result<PostDetailDto>.Fail("You do not have permission to get this post");

        // Return post
        var result = new PostDetailDto
        {
            Id = post.Id,
            ImageUrl = post.ImageUrl,
            Description = post.Description,
            MoodEmote = post.MoodEmote,
            Privacy = post.Privacy,
            Owner = post.Owner,
            Reactions = post.Reactions,
            CreatedAt = post.CreatedAt
        };
        return Result<PostDetailDto>.Ok(result);    
    }
    
    // GET MY POST FEED 
    public async Task<GetFriendFeedResponseDto> GetMyFeedAsync(Guid currentUserId, GetFriendFeedRequestDto request)
    {
        var query = _db.PostFeeds
            .Include(x => x.Post)
                .ThenInclude(x => x.User)
            .Include(x => x.Post)
                .ThenInclude(x => x.MoodEmote)
            .Include(x => x.Post)
                .ThenInclude(x => x.Reactions)
            .Where(x => x.ViewerId == currentUserId && x.Post.DeletedAt == null)
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

        // Sort
        query = query.OrderByDescending(x => x.Post.CreatedAt).ThenByDescending(x => x.Post.Id);

        // Take limit + 1
        var feeds = await query.Take(request.Limit + 1).ToListAsync();

        // Next cursor
        string? nextCursor = null;

        if (feeds.Count > request.Limit)
        {
            var lastVisibleItem = feeds[request.Limit - 1];
            nextCursor = CursorHelper.Encode(lastVisibleItem.Post.CreatedAt, lastVisibleItem.Post.Id);
            feeds.RemoveAt(request.Limit);
        }

        // Map DTO
        var items = feeds.Select(x => new FriendFeedItemDto
        {
            Post = new PostDto
            {
                Id = x.Post.Id,
                ImageUrl = x.Post.ImageUrl,
                Description = x.Post.Description,
                Privacy = x.Post.Privacy,
                CreatedAt = x.Post.CreatedAt,
                ReactionCount = x.Post.Reactions.Count,
                MoodEmote = new EmoteDto
                {
                    Id = x.Post.MoodEmote.Id,
                    Name = x.Post.MoodEmote.Name,
                    ImageUrl = x.Post.MoodEmote.ImageUrl
                },
            },

            Owner = new UserDto
            {
                Id = x.Post.User.Id,
                DisplayName = x.Post.User.DisplayName,
                AvatarUrl = x.Post.User.AvatarUrl
            }
        }).ToList();

        return new GetFriendFeedResponseDto
        {
            Items = items,
            NextCursor = nextCursor
        };
    }
    
    // GET FRIEND POST FEED
    public async Task<Result<GetFriendFeedResponseDto>> GetFriendFeedAsync(Guid currentUserId, Guid profileUserId, GetFriendFeedRequestDto request)
    {
        var query = _db.PostFeeds
            .Include(x => x.Post)
                .ThenInclude(x => x.User)
            .Include(x => x.Post)
                .ThenInclude(x => x.MoodEmote)
            .Include(x => x.Post)
                .ThenInclude(x => x.Reactions)
            .Where(x =>
                x.ViewerId == currentUserId &&
                x.Post.UserId == profileUserId &&
                x.Post.DeletedAt == null);

        // Check if current user access owner posts
        if (currentUserId == profileUserId)
            return Result<GetFriendFeedResponseDto>.Fail("You can not access your posts");
        
        // Cursor pagination
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, postId) = CursorHelper.Decode(request.Cursor);

            query = query.Where(x =>
                x.Post.CreatedAt < createdAt ||
                (x.Post.CreatedAt == createdAt &&
                x.Post.Id.CompareTo(postId) < 0));
        }

        // Sort newest first
        query = query
            .OrderByDescending(x => x.Post.CreatedAt)
            .ThenByDescending(x => x.Post.Id);

        // Take limit + 1
        var feeds = await query.Take(request.Limit + 1).ToListAsync();

        string? nextCursor = null;

        if (feeds.Count > request.Limit)
        {
            var lastVisibleItem = feeds[request.Limit - 1];

            nextCursor = CursorHelper.Encode(lastVisibleItem.Post.CreatedAt, lastVisibleItem.Post.Id);

            feeds.RemoveAt(request.Limit);
        }

        var items = feeds.Select(x => new FriendFeedItemDto
        {
            Post = new PostDto
            {
                Id = x.Post.Id,
                ImageUrl = x.Post.ImageUrl,
                Description = x.Post.Description,
                Privacy = x.Post.Privacy,
                CreatedAt = x.Post.CreatedAt,
                ReactionCount = x.Post.Reactions.Count,
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
        }).ToList();

        var result = new GetFriendFeedResponseDto
        {
            Items = items,
            NextCursor = nextCursor
        };
        return Result<GetFriendFeedResponseDto>.Ok(result);
    }
}