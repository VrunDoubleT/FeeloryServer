namespace FeeloryBackend.Services.Interfaces;

public interface IMissionProgressService
{
    Task ProcessLoginAsync(Guid userId);

    Task ProcessDayShareCreatedAsync(
        Guid userId,
        Guid dayShareId);

    Task ProcessReactionSentAsync(
        Guid userId,
        Guid postId);

    Task ProcessReactionReceivedAsync(
        Guid postOwnerId,
        Guid reactorId,
        Guid postId);
}