using System.ComponentModel.DataAnnotations;
using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.DTOs.DayShare;

public class CreateDayShareRequestDto : IValidatableObject
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

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        // Validate for privacy
        if (Privacy != DayShareTypeConstants.Friends && Privacy != DayShareTypeConstants.Custom)
        {
            yield return new ValidationResult("Invalid privacy type", [nameof(Privacy)]); 
        }

        // Validate allowed user id with custom privacy
        if (Privacy == DayShareTypeConstants.Custom && (AllowedUserIds == null || AllowedUserIds.Count == 0))
        {
            yield return new ValidationResult("At least one allowed friend is required for Custom privacy", [nameof(AllowedUserIds)]);
        }

        if (Privacy == DayShareTypeConstants.Friends && AllowedUserIds != null && AllowedUserIds.Count > 0)
        {
            yield return new ValidationResult("AllowedUserIds can only be provided when privacy is Custom", [nameof(AllowedUserIds)]);
        }
    }
}