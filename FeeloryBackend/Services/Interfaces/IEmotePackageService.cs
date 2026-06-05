using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Emote;

namespace FeeloryBackend.Services.Interfaces;

public interface IEmotePackageService
{
    // Get all packages
    Task<Result<List<EmotePackageDto>>> GetAllAsync();

    // Get package detail
    Task<Result<EmotePackageDto>> GetByIdAsync(Guid packageId);

    // Unlock package for user
    Task<Result> UnlockAsync(Guid userId, Guid packageId);

    // Get user packages
    Task<Result<List<EmotePackageDto>>> GetUserPackagesAsync(Guid userId);
}