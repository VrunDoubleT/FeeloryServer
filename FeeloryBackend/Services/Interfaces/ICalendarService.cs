using FeeloryBackend.Commons;
using FeeloryBackend.Models.DTOs.Calendar;

namespace FeeloryBackend.Services.Interfaces;

public interface ICalendarService
{
    // Get monthly diary summary
    Task<Result<MonthlyCalendarDto>> GetMonthlyAsync(Guid userId, int month, int year);
    
    // Get daily timeline
    Task<Result<DailyTimelineDto>> GetDailyAsync(Guid userId, DateOnly date);
}