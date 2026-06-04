namespace FeeloryBackend.Services.Interfaces;

public interface IPostAccessService
{
    /// <summary>
    /// Checks whether a user has permission to view a post.
    /// Returns true if the post belongs to the user,
    /// the post is public, or the post is visible to friends
    /// and the user is included in PostViewers/PostFeeds.
    /// </summary>
    Task<bool> CanViewPostAsync(Guid postId, Guid requesterId);

    /// <summary>
    /// Checks whether the specified user is the owner of the post.
    /// </summary>
    Task<bool> IsPostOwnerAsync(Guid postId, Guid userId);
}