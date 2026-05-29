using FeeloryBackend.Commons;
using FeeloryBackend.Data;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class EmotePackageService : IEmotePackageService
{
    private readonly AppDbContext _db;
    public EmotePackageService(AppDbContext db) => _db = db;

    public async Task<Result<List<EmotePackageDto>>> GetAllAsync()
    {
        var packages = await _db.EmotePackages
            .AsNoTracking()
            .Select(p => new EmotePackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CoverUrl = p.CoverUrl,
                IsDefault = p.IsDefault,
                Items = p.Items.Select(i => new EmoteDto
                { Id = i.Emote.Id, Name = i.Emote.Name, ImageUrl = i.Emote.ImageUrl }).ToList()
            }).ToListAsync();
        return Result<List<EmotePackageDto>>.Ok(packages);
    }

    public async Task<Result<EmotePackageDto>> GetByIdAsync(Guid packageId)
    {
        var package = await _db.EmotePackages
            .AsNoTracking()
            .Where(p => p.Id == packageId)
            .Select(p => new EmotePackageDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CoverUrl = p.CoverUrl,
                IsDefault = p.IsDefault,
                Items = p.Items.Select(i => new EmoteDto
                { Id = i.Emote.Id, Name = i.Emote.Name, ImageUrl = i.Emote.ImageUrl }).ToList()
            }).FirstOrDefaultAsync();

        if (package is null) return Result<EmotePackageDto>.Fail("No emote pack found.");
        return Result<EmotePackageDto>.Ok(package);
    }

    public async Task<Result> UnlockAsync(Guid userId, Guid packageId)
    {
        var package = await _db.EmotePackages.FindAsync(packageId);
        if (package is null) return Result.Fail("The emote pack does not exist.");

        bool isOwned = await _db.UserPackages.AnyAsync(up => up.UserId == userId && up.PackageId == packageId);
        if (isOwned || package.IsDefault) return Result.Fail("The user already owns this emote pack.");

        _db.UserPackages.Add(new UserPackage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PackageId = packageId,
            UnlockedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Result.Ok();
    }

    public async Task<Result<List<EmotePackageDto>>> GetUserPackagesAsync(Guid userId)
    {
        var unlockedPackages = await _db.UserPackages
            .AsNoTracking()
            .Where(up => up.UserId == userId)
            .Select(up => new EmotePackageDto
            {
                Id = up.Package.Id,
                Name = up.Package.Name,
                Description = up.Package.Description,
                CoverUrl = up.Package.CoverUrl,
                IsDefault = up.Package.IsDefault
            }).ToListAsync();

        return Result<List<EmotePackageDto>>.Ok(unlockedPackages);
    }
}