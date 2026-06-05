using FeeloryBackend.Commons;
using FeeloryBackend.Data;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Calendar;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Post;
using FeeloryBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Services.Implementations;

public class CalendarService : ICalendarService
{
    private readonly AppDbContext _dbContext;

    public CalendarService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Result<MonthlyCalendarDto>> GetMonthlyAsync(Guid userId, int month, int year)
    {
        var postDays = await _dbContext.Posts
            .Where(p =>
                p.UserId == userId &&
                p.DeletedAt == null &&
                p.CreatedAt.Month == month &&
                p.CreatedAt.Year == year)
            .GroupBy(p => p.CreatedAt.Day)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync();
        int daysInMonth = DateTime.DaysInMonth(year, month);
        var postCountPerDay = new Dictionary<int, int>();
        for (int day = 1; day <= daysInMonth; day++)
        {
            postCountPerDay[day] = 0;
        }
        foreach (var entry in postDays)
        {
            postCountPerDay[entry.Day] = entry.Count;
        }
        return Result<MonthlyCalendarDto>.Ok(new MonthlyCalendarDto
        {
            Month = month,
            Year = year,
            PostCountPerDay = postCountPerDay
        });
    }
    
    public async Task<Result<DailyTimelineDto>> GetDailyAsync(Guid userId, DateOnly date)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);
        var posts = await _dbContext.Posts
            .Where(p =>
                p.UserId == userId &&
                p.DeletedAt == null &&
                p.CreatedAt >= startOfDay &&
                p.CreatedAt <= endOfDay)
            .OrderBy(p => p.CreatedAt)
            .Select(p => new PostDto
            {
                Id = p.Id,
                ImageUrl = p.ImageUrl,
                Description = p.Description,
                Privacy = p.Privacy,
                MoodEmote = new EmoteDto
                {
                    Id = p.MoodEmote.Id,
                    Name = p.MoodEmote.Name,
                    ImageUrl = p.MoodEmote.ImageUrl
                },
                // User = new UserDto
                // {
                //     Id = p.User.Id,
                //     Username = p.User.Username,
                //     DisplayName = p.User.DisplayName,
                //     AvatarUrl = p.User.AvatarUrl,
                //     CreatedAt = p.User.CreatedAt
                // },
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
        return Result<DailyTimelineDto>.Ok(new DailyTimelineDto
        {
            Date = date,
            Posts = posts
        });
    }
}