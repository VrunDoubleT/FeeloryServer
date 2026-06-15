using FeeloryBackend.Data;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class MissionInitializationService : IMissionInitializationService
{
    private readonly AppDbContext _context;

    public MissionInitializationService(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task InitializeUserMissionsAsync(Guid userId)
    {
        var missionIds = await _context.UserMissions
            .Where(um => um.UserId == userId)
            .Select(um => um.MissionId)
            .ToListAsync();

        var missions = await _context.Missions
            .Where(m => !missionIds.Contains(m.Id) && m.IsActive)
            .ToListAsync();

        var now = DateTime.UtcNow;

        var userMissions = missions
            .Select(m => new UserMission
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                MissionId = m.Id,
                CurrentValue = 0,
                Status = MissionStatus.InProgress,
                StartedAt = now,
                ExpiredAt = now.AddDays(m.DurationDays),
                CompletedAt = null,
                RewardClaimedAt = null
            })
            .ToList();
        
        if (userMissions.Count == 0)
        {
            return;
        }
        
        await _context.UserMissions.AddRangeAsync(userMissions);
        await _context.SaveChangesAsync();
    }
}