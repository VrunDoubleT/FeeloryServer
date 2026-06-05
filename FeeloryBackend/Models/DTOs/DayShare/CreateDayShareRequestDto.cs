using System.ComponentModel.DataAnnotations;
using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class CreateDayShareRequestDto
{
    [Required(ErrorMessage = "Date is required.")] 
    public DateOnly Date { get; set; }
    
    [Required(ErrorMessage = "At least one post is required.")]
    [MinLength(1, ErrorMessage = "At least one post is required.")]
    public List<Guid> SelectedPostIds  { get; set; } = new();
    
    public List<Guid>? AllowedUserIds { get; set; }
    
    [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Privacy is required.")]
    public string Privacy { get; set; } = DayShareTypeConstants.Friends;
    
}