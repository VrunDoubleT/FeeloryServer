using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Post;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class DayShareDto
{
    public Guid Id { get; set; }
    public UserDto Owner { get; set; } = null!;
    public DateOnly SharedDate { get; set; }
    public List<PostDto> Posts { get; set; } = new();
}