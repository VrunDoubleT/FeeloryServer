using FeeloryBackend.Models.DTOs.Calendar;

namespace FeeloryBackend.Services.Interfaces;

public interface ICalendarService
{
    // Get monthly diary summary
    Task<MonthlyCalendarDto> GetMonthlyAsync(Guid userId, int month, int year);
    
    // Get daily timeline
    Task<DailyTimelineDto> GetDailyAsync(Guid userId, DateOnly date);
}