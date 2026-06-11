using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Task;
using FeeloryBackend.Models.Enums;

namespace FeeloryBackend.Services.Interfaces;

public interface IMissionService
{
    Task<Result<List<MissionDto>>> GetMyMissionsAsync(Guid userId);
    
    Task<Result<MissionDetailDto>> GetMissionDetailAsync(Guid userId, Guid missionId);
    
    Task<Result<List<MissionDto>>> GetCompletedMissionsAsync(Guid userId);
    
    Task<Result<List<MissionDto>>> GetExpiredMissionsAsync(Guid userId);
    
    Task<Result<List<MissionDto>>> GetInProgressMissionsAsync(Guid userId);
    
    Task<Result> ClaimRewardAsync(Guid userId, Guid missionId);
}