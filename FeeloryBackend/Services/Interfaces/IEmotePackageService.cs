using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Services.Interfaces;

public interface IEmotePackageService
{
    // Get all packages
    Task<List<EmotePackageDto>> GetAllAsync();
    
    // Get package detail
    Task<EmotePackageDto> GetByIdAsync(Guid packageId);
    
    // Unlock package for user
    Task UnlockAsync(Guid userId, Guid packageId);
    
    // Get user packages
    Task<List<EmotePackageDto>> GetUserPackagesAsync(Guid userId);
}