namespace FeeloryBackend.Services.Interfaces;

public interface IMissionInitializationService
{
    Task InitializeUserMissionsAsync(Guid userId);
}