namespace FeeloryBackend.Services.Interfaces;

public interface IHeartbeatService
{
    Task TrackLoginAsync(Guid userId);
}