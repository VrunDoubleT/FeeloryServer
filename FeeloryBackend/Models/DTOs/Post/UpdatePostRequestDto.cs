using System.ComponentModel.DataAnnotations;
using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.DTOs.Post;

public class UpdatePostRequestDto : IValidatableObject
{
    public string Description { get; set; }
    public string Privacy { get; set; } = null!;
    public List<Guid>? AllowedUserIds { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        // Validate Description
        if (string.IsNullOrWhiteSpace(Description))
            yield return new ValidationResult("Description is required", [nameof(Description)]);
        
        // Validate Privacy
        var validPrivacy = new[]
        {
            PostPrivacyConstants.Public,
            PostPrivacyConstants.Private,
            PostPrivacyConstants.Custom
        };

        if (string.IsNullOrWhiteSpace(Privacy))
        {
            yield return new ValidationResult("Privacy is required", [nameof(Privacy)]);
        }
        else if (!validPrivacy.Contains(Privacy))
        {
            yield return new ValidationResult("Privacy is invalid", [nameof(Privacy)]);
        }

        // Validate AllowedUserIds only when privacy = CUSTOM
        if (Privacy == PostPrivacyConstants.Custom)
        {
            if (AllowedUserIds == null || !AllowedUserIds.Any())
            {
                yield return new ValidationResult("AllowedUserIds is required", [nameof(AllowedUserIds)]);
            }
            else if (AllowedUserIds.Any(id => id == Guid.Empty))
            {
                yield return new ValidationResult("AllowedUserIds contains invalid user id", [nameof(AllowedUserIds)]);
            }
        }
        else
        {
            if (AllowedUserIds != null && AllowedUserIds.Any())
            {
                yield return new ValidationResult("AllowedUserIds is only allowed when privacy is CUSTOM", [nameof(AllowedUserIds)]);
            }
        }
    }
}