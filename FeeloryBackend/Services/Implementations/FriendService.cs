using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Extensions;
using FeeloryBackend.Helpers;
using FeeloryBackend.Messaging.RabbitMQ.Messages;
using FeeloryBackend.Messaging.RabbitMQ.Publishers;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.Friend;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class FriendService : IFriendService
{
    private readonly AppDbContext _db;
    private readonly NotificationPublisher _notificationPublisher;

    public FriendService(AppDbContext db, NotificationPublisher notificationPublisher)
    {
        _db = db;
        _notificationPublisher = notificationPublisher;
    }

    public async Task<Result> SendRequestAsync(Guid senderId, Guid receiverId)
    {
        if (senderId == receiverId)
            return Result.Fail("You cannot send a friend request to yourself");

        // Are they already friends?
        bool alreadyFriends = await _db.Friends.AreFriendsAsync(senderId, receiverId);
        if (alreadyFriends)
            return Result.Fail("They are already friends");

        // Is there already a pending request?
        bool requestExists = await _db.FriendRequests.AnyAsync(r =>
            ((r.SenderId == senderId && r.ReceiverId == receiverId) ||
             (r.SenderId == receiverId && r.ReceiverId == senderId)) &&
            r.Status == FriendRequestConstants.Pending);

        if (requestExists)
            return Result.Fail("A friend request is already pending");

        // Does the receiver exist?
        bool receiverExists = await _db.Users.AnyAsync(u => u.Id == receiverId);
        if (!receiverExists)
            return Result.Fail("The user does not exist");

        var friendRequest = new FriendRequest()
        {
            Id = Guid.NewGuid(),
            SenderId = senderId,
            ReceiverId = receiverId,
            Status = FriendRequestConstants.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.FriendRequests.Add(friendRequest);
        await _db.SaveChangesAsync();

        // Publish a friend request notification message
        await _notificationPublisher.PublishFriendRequestReceivedAsync(new FriendRequestReceivedMessage()
        {
            SenderId = friendRequest.SenderId,
            ReceiverId = friendRequest.ReceiverId,
            FriendRequestId = friendRequest.Id
        });
        
        return Result.Ok();
    }

    public async Task<Result> AcceptRequestAsync(Guid currentUserId, Guid requestId)
    {
        var request = await _db.FriendRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == currentUserId);

        if (request is null)
            return Result.Fail("The friend request does not exist");

        if (request.Status != FriendRequestConstants.Pending)
            return Result.Fail("This friend request has already been processed");

        bool alreadyFriends = await _db.Friends.AreFriendsAsync(request.SenderId, request.ReceiverId);
        if (alreadyFriends)
            return Result.Fail("They are already friends");
        
        request.Status = FriendRequestConstants.Accepted;

        // Create a Friend relationship using canonical ordering via the factory method
        _db.Friends.Add(Friend.Create(request.SenderId, request.ReceiverId));
        await _db.SaveChangesAsync();

        await _notificationPublisher.PublishFriendRequestAcceptedAsync(new FriendRequestAcceptedMessage()
        {
            SenderId = request.SenderId,
            AccepterId = request.ReceiverId,
            FriendRequestId = requestId
        });
        
        return Result.Ok();
    }

    public async Task<Result> RejectRequestAsync(Guid currentUserId, Guid requestId)
    {
        var request = await _db.FriendRequests
            .FirstOrDefaultAsync(r => r.Id == requestId && r.ReceiverId == currentUserId);

        if (request is null)
            return Result.Fail("Friend request does not exist");

        if (request.Status != FriendRequestConstants.Pending)
            return Result.Fail("This friend request has already been processed");

        request.Status = FriendRequestConstants.Rejected;

        await _db.SaveChangesAsync();
        return Result.Ok();
    }
    
    public async Task<Result> RemoveFriendAsync(Guid currentUserId, Guid friendUserId)
    {
        var friendship = await _db.Friends
            .BetweenUsers(currentUserId, friendUserId)
            .FirstOrDefaultAsync();

        if (friendship is null)
            return Result.Fail("The friendship relationship was not found");

        _db.Friends.Remove(friendship);
        await _db.SaveChangesAsync();
        return Result.Ok();
    }
    
    public async Task<Result<CursorPaginationResponse<FriendDto>>>
        GetFriendsAsync(Guid userId, CursorPaginationRequest request)
    {
        var query = _db.Friends
            .AsNoTracking()
            .GetFriendsOfUser(userId)
            .OrderByDescending(f => f.CreatedAt)
            .ThenByDescending(f => f.Id)
            .AsQueryable();

        // Apply cursor
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) =
                CursorHelper.Decode(request.Cursor);

            query = query.Where(f =>
                f.CreatedAt < createdAt
                || (
                    f.CreatedAt == createdAt &&
                    f.Id.CompareTo(id) < 0
                ));
        }

        // Get extra item
        var friends = await query
            .Select(f => new FriendDto
            {
                Id = f.Id,

                User = new UserDto
                {
                    Id = f.UserId == userId
                        ? f.FriendUser.Id
                        : f.User.Id,

                    Username = f.UserId == userId
                        ? f.FriendUser.Username
                        : f.User.Username,

                    DisplayName = f.UserId == userId
                        ? f.FriendUser.DisplayName
                        : f.User.DisplayName,

                    AvatarUrl = f.UserId == userId
                        ? f.FriendUser.AvatarUrl
                        : f.User.AvatarUrl,

                    CreatedAt = f.UserId == userId
                        ? f.FriendUser.CreatedAt
                        : f.User.CreatedAt
                },

                CreatedAt = f.CreatedAt
            })
            .Take(request.PageSize + 1)
            .ToListAsync();

        bool hasNextPage =
            friends.Count > request.PageSize;

        friends = friends
            .Take(request.PageSize)
            .ToList();

        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = friends.Last();

            nextCursor = CursorHelper.Encode(
                lastItem.CreatedAt,
                lastItem.Id
            );
        }

        var response = new CursorPaginationResponse<FriendDto>(
                friends,
                nextCursor,
                hasNextPage,
                message:"Friends list retrieved successfully"
            );

        return Result<CursorPaginationResponse<FriendDto>>.Ok(response);
    }

    public async Task<Result<CursorPaginationResponse<FriendRequestDto>>>
        GetPendingRequestsAsync(Guid userId, CursorPaginationRequest request)
    {
        var query = _db.FriendRequests
            .AsNoTracking()
            .Where(r =>
                r.ReceiverId == userId &&
                r.Status == FriendRequestConstants.Pending)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id)
            .AsQueryable();

        // Apply cursor
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) =
                CursorHelper.Decode(request.Cursor);
            
            Console.WriteLine(createdAt +" "+ id);

            query = query.Where(r =>
                r.CreatedAt < createdAt
                || (
                    r.CreatedAt == createdAt &&
                    r.Id.CompareTo(id) < 0
                ));
        }

        // Take extra item
        var requests = await query
            .Select(r => new FriendRequestDto
            {
                Id = r.Id,

                Sender = new UserDto
                {
                    Id          = r.Sender.Id,
                    Username    = r.Sender.Username,
                    DisplayName = r.Sender.DisplayName,
                    AvatarUrl   = r.Sender.AvatarUrl,
                    CreatedAt   = r.Sender.CreatedAt
                },

                CreatedAt = r.CreatedAt
            })
            .Take(request.PageSize + 1)
            .ToListAsync();

        bool hasNextPage = requests.Count > request.PageSize;

        requests = requests
            .Take(request.PageSize)
            .ToList();

        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = requests.Last();

            nextCursor = CursorHelper.Encode(
                lastItem.CreatedAt,
                lastItem.Id
            );
        }

        var response = new CursorPaginationResponse<FriendRequestDto>(
                requests,
                nextCursor,
                hasNextPage,
                message:"Successfully retrieved the friend request list"
            );

        return Result<CursorPaginationResponse<FriendRequestDto>>.Ok(response);
    }
}