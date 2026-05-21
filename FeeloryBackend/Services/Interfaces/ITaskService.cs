using FeeloryBackend.Models.DTOs.Task;

namespace FeeloryBackend.Services.Interfaces;

public interface ITaskService
{
    // Get all tasks
    Task<List<TaskDto>> GetAllAsync();
    
    // Get user progress
    Task<List<UserTaskProgressDto>> GetProgressAsync(Guid userId);

    // Update task progress (internal system)
    Task UpdateProgressAsync(Guid userId, string metricKey, int value);

    // Claim reward
    Task ClaimRewardAsync(Guid userId, Guid taskId);
}