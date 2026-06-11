using FeeloryBackend.Commons;
using FeeloryBackend.Data;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Task;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class MissionService : IMissionService
{
    private readonly AppDbContext _db;

    public MissionService(AppDbContext db)
    {
        _db = db;
    }
    
    // Get mission of current user
    public async Task<Result<List<MissionDto>>> GetMyMissionsAsync(Guid userId)
    {
        var missions = await _db.UserMissions
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Status)
            .ThenBy(x => x.ExpiredAt)
            .Select(x => new MissionDto
            {
                MissionId = x.MissionId,
                Name = x.Mission.Name,
                Description = x.Mission.Description,
                CurrentValue = x.CurrentValue,
                TargetValue = x.Mission.TargetValue,
                Status = x.Status,
                StartedAt = x.StartedAt,
                ExpiredAt = x.ExpiredAt,
                CompletedAt = x.CompletedAt,
                RewardClaimedAt = x.RewardClaimedAt,
                Rewards = x.Mission.Rewards.Select(r => new MissionRewardDto
                {
                    PackageId = r.PackageId,
                    PackageName = r.Package.Name,
                    Description = r.Package.Description,
                    CoverUrl = r.Package.CoverUrl
                }).ToList()
            }).ToListAsync();

        return Result<List<MissionDto>>.Ok(missions);
    }
    
    // Get mission detail
    public async Task<Result<MissionDetailDto>> GetMissionDetailAsync(Guid userId, Guid missionId)
    {
        var mission = await _db.UserMissions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.MissionId == missionId)
            .Select(x => new MissionDetailDto
            {
                MissionId = x.MissionId,
                Name = x.Mission.Name,
                Description = x.Mission.Description,
                CurrentValue = x.CurrentValue,
                TargetValue = x.Mission.TargetValue,
                Status = x.Status,
                StartedAt = x.StartedAt,
                ExpiredAt = x.ExpiredAt,
                CompletedAt = x.CompletedAt,
                RewardClaimedAt = x.RewardClaimedAt,
                Rewards = x.Mission.Rewards.Select(r => new MissionRewardDetailDto
                {
                    PackageId = r.PackageId,
                    PackageName = r.Package.Name,
                    Emotes = r.Package.Items.Select(i => new EmoteDto
                    {
                        Id = i.Emote.Id,
                        Name = i.Emote.Name,
                        ImageUrl = i.Emote.ImageUrl
                    }).ToList()
                }).ToList()
            }).FirstOrDefaultAsync();
        
        return Result<MissionDetailDto>.Ok(mission);
    }
    
    // Get mission by status
    private async Task<List<MissionDto>> GetMissionsByStatusAsync(Guid userId, MissionStatus status)
    {
        return await _db.UserMissions
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.Status == status)
            .Select(x => new MissionDto
            {
                MissionId = x.MissionId,
                Name = x.Mission.Name,
                Description = x.Mission.Description,
                CurrentValue = x.CurrentValue,
                TargetValue = x.Mission.TargetValue,
                Status = x.Status,
                StartedAt = x.StartedAt,
                ExpiredAt = x.ExpiredAt,
                CompletedAt = x.CompletedAt,
                RewardClaimedAt = x.RewardClaimedAt,
                Rewards = x.Mission.Rewards.Select(r => new MissionRewardDto
                {
                    PackageId = r.PackageId,
                    PackageName = r.Package.Name,
                    Description = r.Package.Description,
                    CoverUrl = r.Package.CoverUrl
                }).ToList()
            }).ToListAsync();
    }
    
    // Status = COMPLETED 
    public async Task<Result<List<MissionDto>>> GetCompletedMissionsAsync(Guid userId)
    {
        var missions = await GetMissionsByStatusAsync(userId, MissionStatus.Completed);
        if (!missions.Any())
            return Result<List<MissionDto>>.Fail("Completed mission not found");
        return Result<List<MissionDto>>.Ok(missions);
    }
    
    // Status = EXPIRED
    public async Task<Result<List<MissionDto>>> GetExpiredMissionsAsync(Guid userId)
    {
        var missions = await GetMissionsByStatusAsync(userId, MissionStatus.Expired);
        if (!missions.Any())
            return Result<List<MissionDto>>.Fail("Expired mission not found");
        return Result<List<MissionDto>>.Ok(missions);
    }
    
    // Status = IN-PROGRESS
    public async Task<Result<List<MissionDto>>> GetInProgressMissionsAsync(Guid userId)
    {
        return Result<List<MissionDto>>.Ok(await GetMissionsByStatusAsync(userId, MissionStatus.InProgress));
    }
    
    // Claim reward
    public async Task<Result> ClaimRewardAsync(Guid userId, Guid missionId)
    {
        var userMission = await _db.UserMissions
            .Include(x => x.Mission)
            .ThenInclude(x => x.Rewards)
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.MissionId == missionId);

        if (userMission.Status == MissionStatus.Claimed)
            return Result.Fail("Mission is already claimed");
        
        if (userMission.Status != MissionStatus.Completed)
            return Result.Fail("Mission is not completed");
        
        foreach (var reward in userMission.Mission.Rewards)
        {
            bool alreadyOwned = await _db.UserPackages.AnyAsync(x =>
                x.UserId == userId &&
                x.PackageId == reward.PackageId);

            if (alreadyOwned)
                continue;
            
            _db.UserPackages.Add(new UserPackage
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = reward.PackageId,
                UnlockedAt = DateTime.UtcNow
            });
        }

        userMission.Status = MissionStatus.Claimed;
        userMission.RewardClaimedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result.Ok();
    }
}