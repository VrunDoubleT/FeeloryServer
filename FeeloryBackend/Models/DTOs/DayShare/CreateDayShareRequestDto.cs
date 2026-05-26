using System.ComponentModel.DataAnnotations;
using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class CreateDayShareRequestDto
{
    public DateOnly Date { get; set; }
    public List<Guid> SelectedPostIds  { get; set; } = new();
    
    [MinLength(1, ErrorMessage = "At least one post is required.")]
    public List<Guid>? AllowedUserIds { get; set; }
    public string? Description { get; set; }
    public string Privacy { get; set; } = DayShareTypeConstants.Friends;
    
}