namespace FeeloryBackend.Models.DTOs.Calendar;

public class MonthlyCalendarDto
{
    public int Month { get; set; }
    public int Year { get; set; }

    public Dictionary<int, int> PostCountPerDay { get; set; } = new();
}