using FeeloryBackend.Constants;
using FeeloryBackend.Data;
using FeeloryBackend.Models.Entities;
using FeeloryBackend.Models.Enums;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class MissionProgressService : IMissionProgressService
{
    private readonly AppDbContext _context;

    public MissionProgressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task ProcessLoginAsync(Guid userId)
    {
        var missions = await _context.UserMissions
            .Include(x => x.Mission)
            .ThenInclude(x => x.MissionType)
            .Where(x =>
                x.UserId == userId &&
                x.Status == MissionStatus.InProgress &&
                x.ExpiredAt > DateTime.UtcNow &&
                x.Mission.MissionType.MetricKey ==
                MissionMetricKeyConstants.Login)
            .ToListAsync();

        foreach (var mission in missions)
        {
            mission.CurrentValue++;

            if (mission.CurrentValue >= mission.Mission.TargetValue)
            {
                mission.CurrentValue = mission.Mission.TargetValue;
                mission.Status = MissionStatus.Completed;
                mission.CompletedAt ??= DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task ProcessDayShareCreatedAsync(
        Guid userId,
        Guid dayShareId)
    {
        var dayShare = await _context.DayShares
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == dayShareId && x.DeletedAt == null);

        if (dayShare == null)
        {
            return;
        }

        var alreadySharedThisDate =
            await _context.DayShares
                .AsNoTracking()
                .AnyAsync(x =>
                    x.OwnerId == userId &&
                    x.Id != dayShareId &&
                    x.SharedDate.Date == dayShare.SharedDate.Date);


        if (alreadySharedThisDate)
        {
            return;
        }

        var missions = await _context.UserMissions
            .Include(x => x.Mission)
            .ThenInclude(x => x.MissionType)
            .Where(x =>
                x.UserId == userId &&
                x.Status == MissionStatus.InProgress &&
                x.Mission.MissionType.MetricKey == MissionMetricKeyConstants.DayShareCreated)
            .ToListAsync();

        foreach (var mission in missions)
        {
            mission.CurrentValue++;

            if (mission.CurrentValue >= mission.Mission.TargetValue)
            {
                mission.Status = MissionStatus.Completed;
                mission.CompletedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task ProcessReactionSentAsync(
        Guid userId,
        Guid postId)
    {
        var missions = await _context.UserMissions
            .Include(x => x.Mission)
            .ThenInclude(x => x.MissionType)
            .Where(x =>
                x.UserId == userId &&
                x.Status == MissionStatus.InProgress &&
                x.Mission.MissionType.MetricKey == MissionMetricKeyConstants.ReactionSent)
            .ToListAsync();

        foreach (var mission in missions)
        {
            bool exists =
                await _context.UserMissionReactionHistories
                    .AnyAsync(x =>
                        x.UserId == userId &&
                        x.MissionId == mission.MissionId &&
                        x.PostId == postId);

            if (exists)
            {
                continue;
            }

            _context.UserMissionReactionHistories.Add(
                new UserMissionReactionHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    MissionId = mission.MissionId,
                    PostId = postId,
                    ReactorId = null
                });

            mission.CurrentValue++;

            if (mission.CurrentValue >= mission.Mission.TargetValue)
            {
                mission.Status = MissionStatus.Completed;
                mission.CompletedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task ProcessReactionReceivedAsync(
        Guid postOwnerId,
        Guid reactorId,
        Guid postId)
    {
        var missions = await _context.UserMissions
            .Include(x => x.Mission)
            .ThenInclude(x => x.MissionType)
            .Where(x =>
                x.UserId == postOwnerId &&
                x.Status == MissionStatus.InProgress &&
                x.Mission.MissionType.MetricKey ==
                MissionMetricKeyConstants.ReactionReceived)
            .ToListAsync();

        if (missions.Count == 0)
        {
            return;
        }

        var missionIds = missions
            .Select(x => x.MissionId)
            .ToList();

        var existingMissionIds = (
                await _context.UserMissionReactionHistories
                    .Where(x =>
                        x.UserId == postOwnerId &&
                        x.PostId == postId &&
                        x.ReactorId == reactorId &&
                        missionIds.Contains(x.MissionId))
                    .Select(x => x.MissionId)
                    .ToListAsync())
            .ToHashSet();

        foreach (var mission in missions)
        {
            if (existingMissionIds.Contains(mission.MissionId))
            {
                continue;
            }

            _context.UserMissionReactionHistories.Add(
                new UserMissionReactionHistory
                {
                    Id = Guid.NewGuid(),
                    UserId = postOwnerId,
                    MissionId = mission.MissionId,
                    PostId = postId,
                    ReactorId = reactorId
                });

            mission.CurrentValue++;

            if (mission.CurrentValue >= mission.Mission.TargetValue)
            {
                mission.CurrentValue = mission.Mission.TargetValue;
                mission.Status = MissionStatus.Completed;
                mission.CompletedAt ??= DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }
}