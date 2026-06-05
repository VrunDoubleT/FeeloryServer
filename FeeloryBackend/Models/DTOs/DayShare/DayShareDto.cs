using FeeloryBackend.Constants;
using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareDto
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string? Description { get; set; }
    public DateTime SharedDate { get; set; }
    public string ShareType { get; set; } = DayShareTypeConstants.Friends;
}