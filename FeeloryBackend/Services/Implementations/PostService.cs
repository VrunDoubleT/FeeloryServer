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

    // CREATE
    public async Task<Guid> CreateAsync(Guid userId, CreatePostRequestDto request)
    {
        // Validate mood emote
        bool ownsMoodEmote = await _db.UserPackages
            .AnyAsync(up =>
                up.UserId == userId &&
                up.Package.Items.Any(item =>
                    item.EmoteId == request.MoodEmoteId));

        if (!ownsMoodEmote)
            return Guid.Empty;

        // Validate privacy
        var validPrivacy = new[]
        {
            PostPrivacyConstants.Private,
            PostPrivacyConstants.Public,
            PostPrivacyConstants.Custom
        };

        if (!validPrivacy.Contains(request.Privacy))
            return Guid.Empty;

        // CUSTOM requires allowed users
        if (request.Privacy == PostPrivacyConstants.Custom)
        {
            if (request.AllowedUserIds == null ||
                !request.AllowedUserIds.Any())
                return Guid.Empty;

            // Check all selected users exist
            var existingUsers = await _db.Users
                .CountAsync(u =>
                    request.AllowedUserIds.Contains(u.Id));

            if (existingUsers != request.AllowedUserIds.Count)
                return Guid.Empty;

            // Check all selected users are friends
            foreach (var viewerId in request.AllowedUserIds)
            {
                bool areFriends = await _db.Friends.AreFriendsAsync(
                    userId,
                    viewerId
                );

                if (!areFriends)
                    return Guid.Empty;
            }
        }

        // Upload image to cloudinary
        var imageUrl = await _cloudinaryService
            .UploadImageAsync(request.Image);

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

        // Add viewers if CUSTOM
        if (request.Privacy == PostPrivacyConstants.Custom)
        {
            var viewers = request.AllowedUserIds!
                .Select(viewerId => new PostViewer
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    ViewerId = viewerId
                });

            await _db.PostViewers.AddRangeAsync(viewers);
        }

        await _db.SaveChangesAsync();

        // Publish RabbitMQ
        await _postPublisher.PublishPostAsync(
                new PostCreatedMessage
                {
                    PostId = post.Id,
                    UserId = userId,
                    Privacy = request.Privacy,
                    AllowedUserIds = request.AllowedUserIds,
                    CreatedAt = post.CreatedAt
                });
        
        return post.Id;
    }

    // UPDATE
    public async Task<bool> UpdateAsync(Guid userId, Guid postId, UpdatePostRequestDto request)
    {
        // Find post
        var post = await _db.Posts
            .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId);

        if (post == null) return false;

        // Validate privacy
        var validPrivacy = new[]
        {
            PostPrivacyConstants.Private,
            PostPrivacyConstants.Public,
            PostPrivacyConstants.Custom
        };
        if (!validPrivacy.Contains(request.Privacy)) return false;

        // Current viewers
        var oldViewerIds = (await _db.PostViewers
                .Where(x => x.PostId == postId)
                .Select(x => x.ViewerId)
                .ToListAsync()
        ).ToHashSet();
        var newViewerIds = new HashSet<Guid>();

        // Validate CUSTOM
        if (request.Privacy == PostPrivacyConstants.Custom)
        {
            if (request.AllowedUserIds == null || !request.AllowedUserIds.Any())
            {
                return false;
            }

            // Check user exists
            var existingUsers = await _db.Users
                .CountAsync(u => request.AllowedUserIds.Contains(u.Id));

            if (existingUsers != request.AllowedUserIds.Count)
            {
                return false;
            }

            // Check friendship
            foreach (var viewerId in request.AllowedUserIds)
            {
                bool areFriends = await _db.Friends
                .AreFriendsAsync(userId, viewerId);

                if (!areFriends) return false;
            }

            newViewerIds = request.AllowedUserIds.ToHashSet();
        }

        // Calculate diff
        var removedUsers = oldViewerIds.Except(newViewerIds).ToList();
        var addedUsers = newViewerIds.Except(oldViewerIds).ToList();

        // Update post
        post.Description = request.Description;
        post.Privacy = request.Privacy;

        // Update viewers
        var oldViewers = await _db.PostViewers
            .Where(x => x.PostId == postId)
            .ToListAsync();
        _db.PostViewers.RemoveRange(oldViewers);

        if (request.Privacy == PostPrivacyConstants.Custom)
        {
            var viewers = newViewerIds
            .Select(
                viewerId => new PostViewer
                {
                    Id = Guid.NewGuid(),
                    PostId = post.Id,
                    ViewerId = viewerId
                });

            await _db.PostViewers.AddRangeAsync(viewers);
        }

        await _db.SaveChangesAsync();

        // Publish RabbitMQ
        if (addedUsers.Any() || removedUsers.Any())
        {
            await _postPublisher.PublishPermissionChangedAsync(

            new PostPermissionChangedMessage
            {
                PostId = post.Id,
                AddedUserIds = addedUsers,
                RemovedUserIds = removedUsers
            });
        }

        return true;
    }

    // DELETE
    public async Task<bool> DeleteAsync(Guid userId, Guid postId)
    {
        var post = await _db.Posts
                .FirstOrDefaultAsync(p => p.Id == postId && p.UserId == userId && p.DeletedAt == null);

        if (post == null)
        {
            return false;
        }

        // Soft delete
        post.DeletedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Publish RabbitMQ
        await _postPublisher.PublishPostDeletedAsync(
            new PostDeletedMessage
                {
                    PostId = post.Id
                }
            );

        return true;
    }

    // GET POSTS BY USER
    public async Task<GetMyPostsResponseDto> GetMyPostsAsync(Guid userId, GetMyPostsRequestDto request)
    {
        var query = _db.Posts
            .Include(x => x.MoodEmote)
            .Include(x => x.Reactions)
            .Where(x => x.UserId == userId && x.DeletedAt == null)
            .AsQueryable();

        // Filter date
        if (request.Date.HasValue)
        {
            var date = request.Date.Value.Date;
            query = query.Where(x => x.CreatedAt.Date == date);
        }

        // Filter privacy
        if (!string.IsNullOrWhiteSpace(request.Privacy))
        {
            query = query.Where(x => x.Privacy == request.Privacy);
        }

        // Cursor
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) = CursorHelper.Decode(request.Cursor);

            query = query
                .Where(x => x.CreatedAt < createdAt || (x.CreatedAt == createdAt && x.Id.CompareTo(id) < 0));
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
            var nextItem = posts.Last();
            nextCursor = CursorHelper.Encode(nextItem.CreatedAt, nextItem.Id);
            posts.RemoveAt(posts.Count - 1);
        }

        return new GetMyPostsResponseDto
        {
            Items = posts,
            NextCursor = nextCursor
        };
    }
    
    // GET POSTS BY ID
    public async Task<PostDetailDto?> GetByIdAsync(Guid currentUserId, Guid postId)
    {
        // Find post
        var post = await _db.Posts
            .Include(x => x.User)
            .Include(x => x.MoodEmote)
            .Include(x => x.Reactions)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == postId && x.DeletedAt == null);

        if (post == null)
        {
            return null;
        }

        // Owner
        bool isOwner = post.UserId == currentUserId;

        // In feed
        bool inFeed = await _db.PostFeeds.AnyAsync(x => x.PostId == postId && x.ViewerId == currentUserId);

        // No permission
        if (!isOwner && !inFeed)
        {
            throw new UnauthorizedAccessException();
        }
        
        // Return post
        return new PostDetailDto
        {
            Id = post.Id,
            ImageUrl = post.ImageUrl,
            Description = post.Description,
            Privacy = post.Privacy,
            MoodEmote = post.MoodEmote.Name,
            CreatedAt = post.CreatedAt,
            Owner = new UserDto
            {
                Id = post.User.Id,
                DisplayName = post.User.DisplayName,
                AvatarUrl = post.User.AvatarUrl
            },
            Reactions = post.Reactions.Select(x => new PostReactionDto 
                {
                    UserId = x.UserId,
                    DisplayName = x.User.DisplayName,
                    Reaction = x.Emote.Name
                }).ToList()
        };
    }
    
    // GET FRIEND POST FEED
    public async Task<GetFriendFeedResponseDto> GetFriendFeedAsync(Guid currentUserId, GetFriendFeedRequestDto request)
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

        // Cursor
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) = CursorHelper.Decode(request.Cursor);

            query = query.Where(x => x.Post.CreatedAt < createdAt ||
                                     (x.Post.CreatedAt == createdAt && x.Post.Id.CompareTo(id) < 0));
        }

        // Sort
        query = query.OrderByDescending(x => x.Post.CreatedAt).ThenByDescending(x => x.Post.Id);

        // Take limit + 1
        var feeds = await query.Take(request.Limit + 1).ToListAsync();

        // Next cursor
        string? nextCursor = null;

        if (feeds.Count > request.Limit)
        {
            var nextItem = feeds.Last();

            nextCursor = CursorHelper.Encode(nextItem.Post.CreatedAt, nextItem.Post.Id);

            feeds.RemoveAt(feeds.Count - 1);
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
}