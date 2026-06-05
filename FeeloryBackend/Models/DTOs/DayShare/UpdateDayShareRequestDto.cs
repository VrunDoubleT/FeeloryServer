using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class UpdateDayShareRequestDto
{
    [Required(ErrorMessage = "DayShareId is required.")]
    public Guid DayShareId { get; set; }
    
    [MaxLength(500, ErrorMessage = "Description must not exceed 500 characters.")]
    public string? Description { get; set; }
    
    [Required(ErrorMessage = "Privacy is required.")]
    public string Privacy { get; set; } = string.Empty;
    public List<Guid>? AllowedUserIds { get; set; }
    
    public List<Guid>? SelectedPostIds { get; set; }
}