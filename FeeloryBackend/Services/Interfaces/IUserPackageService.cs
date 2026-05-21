using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Services.Interfaces;

public interface IUserPackageService
{
    // Get user inventory
    Task<List<EmotePackageDto>> GetInventoryAsync(Guid userId);

    // Check ownership
    Task<bool> HasPackageAsync(Guid userId, Guid packageId);

    // Add package (reward system)
    Task AddPackageAsync(Guid userId, Guid packageId);
}