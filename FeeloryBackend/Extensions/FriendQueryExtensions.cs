using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Extensions;

using Microsoft.EntityFrameworkCore;

public static class FriendQueryExtensions
{
    // Get all friend IDs of a specific user (IQueryable version)
    public static IQueryable<Guid> GetFriendIdsOfUser(
        this IQueryable<Friend> query, Guid userId)
    {
        return query
            .GetFriendsOfUser(userId)
            .Select(f => f.UserId == userId ? f.FriendId : f.UserId);
    }

    // Async version: get all friend IDs as List<Guid>
    public static Task<List<Guid>> GetFriendIdsOfUserAsync(
        this IQueryable<Friend> query, Guid userId,
        CancellationToken cancellationToken = default)
    {
        return query
            .GetFriendIdsOfUser(userId)
            .ToListAsync(cancellationToken);
    }
    
    // Get all friend relationships of a specific user
    // This method returns IQueryable<Friend> so EF Core can translate it to SQL
    public static IQueryable<Friend> GetFriendsOfUser(
        this IQueryable<Friend> query, Guid userId)
    {
        return query
            .AsNoTracking()
            .Where(f => f.UserId == userId || f.FriendId == userId);
    }

    // Async version: get all friend relationships as List<Friend>
    public static Task<List<Friend>> GetFriendsOfUserAsync(
        this IQueryable<Friend> query, Guid userId,
        CancellationToken cancellationToken = default)
    {
        return query
            .GetFriendsOfUser(userId)
            .ToListAsync(cancellationToken);
    }

    // Get friendship record between two users
    // This method returns IQueryable<Friend> to allow chaining (AnyAsync, FirstOrDefaultAsync...)
    public static IQueryable<Friend> BetweenUsers(
        this IQueryable<Friend> query, Guid userA, Guid userB)
    {
        var (small, large) = userA.CompareTo(userB) < 0
            ? (userA, userB)
            : (userB, userA);

        return query
            .AsNoTracking()
            .Where(f => f.UserId == small && f.FriendId == large);
    }

    // Async version: get friendship record (FirstOrDefaultAsync)
    public static Task<Friend?> BetweenUsersAsync(
        this IQueryable<Friend> query, Guid userA, Guid userB,
        CancellationToken cancellationToken = default)
    {
        return query
            .BetweenUsers(userA, userB)
            .FirstOrDefaultAsync(cancellationToken);
    }

    // Synchronous check if two users are friends
    public static bool AreFriends(
        this IQueryable<Friend> query, Guid userA, Guid userB)
    {
        return query.BetweenUsers(userA, userB).Any();
    }

    // Async check if two users are friends (recommended in Web API)
    public static Task<bool> AreFriendsAsync(
        this IQueryable<Friend> query, Guid userA, Guid userB,
        CancellationToken cancellationToken = default)
    {
        return query.BetweenUsers(userA, userB)
            .AnyAsync(cancellationToken);
    }
}