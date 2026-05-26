using System.Text.RegularExpressions;
using FeeloryBackend.Commons;
using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Helpers;
using FeeloryBackend.Models.DTOs.Commons;
using FeeloryBackend.Models.DTOs.User;
using FeeloryBackend.Responses;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db) => _db = db;

    public async Task<Result<UserProfileDto>> GetProfileAsync(Guid currentUserId, Guid targetUserId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (user is null)
            return Result<UserProfileDto>.Fail("User not found");

        var friendStatus = await CalculateFriendStatusAsync(currentUserId, targetUserId);

        return Result<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
            FriendStatus = friendStatus
        });
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(Guid currentUserId, UpdateUserRequestDto request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
        if (user is null)
            return Result<UserProfileDto>.Fail("User not found");

        // Validate Username
        if (!string.IsNullOrWhiteSpace(request.Username))
        {
            if (!Regex.IsMatch(request.Username, @"^[a-zA-Z0-9_]{3,30}$"))
                return Result<UserProfileDto>.Fail("Usernames can only contain letters, numbers, underscores, and be between 3 and 30 characters long.");

            bool isUsernameTaken = await _db.Users.AnyAsync(u => u.Username == request.Username && u.Id != currentUserId);
            if (isUsernameTaken)
                return Result<UserProfileDto>.Fail("This username is already in use.");

            user.Username = request.Username;
        }

        // Validate DisplayName
        if (!string.IsNullOrWhiteSpace(request.DisplayName))
        {
            if (request.DisplayName.Length < 1 || request.DisplayName.Length > 50)
                return Result<UserProfileDto>.Fail("DisplayName must be between 1 and 50 characters.");

            user.DisplayName = request.DisplayName;
        }

        if (request.AvatarUrl != null)
            user.AvatarUrl = request.AvatarUrl;

        await _db.SaveChangesAsync();

        return Result<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Username = user.Username,
            AvatarUrl = user.AvatarUrl,
            FriendStatus = "none"
        });
    }

    public async Task<Result<CursorPaginationResponse<UserProfileDto>>> SearchByDisplayNameAsync(Guid currentUserId, string q, CursorPaginationRequest request)
    {
        var query = _db.Users
            .AsNoTracking()
            .Where(u => u.Id != currentUserId && EF.Functions.Like(u.DisplayName, $"%{q}%"))
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .AsQueryable();

        return await ExecuteSearchAsync(currentUserId, query, request);
    }

    public async Task<Result<CursorPaginationResponse<UserProfileDto>>> SearchByUsernameAsync(Guid currentUserId, string username, CursorPaginationRequest request)
    {
        var cleanUsername = username.StartsWith("@") ? username.Substring(1) : username;

        var query = _db.Users
            .AsNoTracking()
            .Where(u => u.Id != currentUserId && (u.Username == cleanUsername || EF.Functions.Like(u.Username, $"%{cleanUsername}%")))
            .OrderByDescending(u => u.CreatedAt)
            .ThenByDescending(u => u.Id)
            .AsQueryable();

        return await ExecuteSearchAsync(currentUserId, query, request);
    }

    // --- Helper Methods ---

    private async Task<string> CalculateFriendStatusAsync(Guid currentUserId, Guid targetUserId)
    {
        if (currentUserId == targetUserId) return "none";

        var minId = currentUserId < targetUserId ? currentUserId : targetUserId;
        var maxId = currentUserId > targetUserId ? currentUserId : targetUserId;

        bool isFriend = await _db.Friends.AnyAsync(f => f.UserId == minId && f.FriendId == maxId);
        if (isFriend) return "friend";

        bool isPending = await _db.FriendRequests.AnyAsync(r =>
            r.Status == FriendRequestConstants.Pending &&
            ((r.SenderId == currentUserId && r.ReceiverId == targetUserId) ||
             (r.SenderId == targetUserId && r.ReceiverId == currentUserId)));

        return isPending ? "pending" : "none";
    }

    private async Task<Result<CursorPaginationResponse<UserProfileDto>>> ExecuteSearchAsync(
        Guid currentUserId, IQueryable<Models.Entities.User> query, CursorPaginationRequest request)
    {
        // Apply cursor logic exactly like FriendService
        if (!string.IsNullOrWhiteSpace(request.Cursor))
        {
            var (createdAt, id) = CursorHelper.Decode(request.Cursor);

            query = query.Where(u =>
                u.CreatedAt < createdAt
                || (
                    u.CreatedAt == createdAt &&
                    u.Id.CompareTo(id) < 0
                ));
        }

        // Take extra item
        var users = await query
            .Take(request.PageSize + 1)
            .ToListAsync();

        bool hasNextPage = users.Count > request.PageSize;

        users = users
            .Take(request.PageSize)
            .ToList();

        string? nextCursor = null;

        if (hasNextPage)
        {
            var lastItem = users.Last();
            nextCursor = CursorHelper.Encode(lastItem.CreatedAt, lastItem.Id);
        }

        // Optimization: Query once to retrieve all friend statuses for the list
        var targetUserIds = users.Select(u => u.Id).ToList();

        var friendships = await _db.Friends
            .AsNoTracking()
            .Where(f => (f.UserId == currentUserId && targetUserIds.Contains(f.FriendId)) ||
                        (f.FriendId == currentUserId && targetUserIds.Contains(f.UserId)))
            .ToListAsync();

        var pendingRequests = await _db.FriendRequests
            .AsNoTracking()
            .Where(r => r.Status == FriendRequestConstants.Pending &&
                        ((r.SenderId == currentUserId && targetUserIds.Contains(r.ReceiverId)) ||
                         (r.ReceiverId == currentUserId && targetUserIds.Contains(r.SenderId))))
            .ToListAsync();

        var dtos = users.Select(user => {
            var minId = currentUserId < user.Id ? currentUserId : user.Id;
            var maxId = currentUserId > user.Id ? currentUserId : user.Id;

            bool isFriend = friendships.Any(f => f.UserId == minId && f.FriendId == maxId);
            bool isPending = pendingRequests.Any(r =>
                (r.SenderId == currentUserId && r.ReceiverId == user.Id) ||
                (r.SenderId == user.Id && r.ReceiverId == currentUserId));

            return new UserProfileDto
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                FriendStatus = isFriend ? "friend" : (isPending ? "pending" : "none")
            };
        }).ToList();

        var response = new CursorPaginationResponse<UserProfileDto>(
            dtos,
            nextCursor,
            hasNextPage
        );

        return Result<CursorPaginationResponse<UserProfileDto>>.Ok(response);
    }
}