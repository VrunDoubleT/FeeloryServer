using System.ComponentModel.DataAnnotations;
using FeeloryBackend.Constants;
using FeeloryBackend.Models.DTOs.Commons;

namespace FeeloryBackend.Models.DTOs.Post;

public class GetMyPostsRequestDto : CursorPaginationRequest, IValidatableObject
{
    public DateTime? Date { get; set; }
    public string? Privacy { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        // Validate Privacy
        if (!string.IsNullOrWhiteSpace(Privacy))
        {
            var validPrivacy = new[]
            {
                PostPrivacyConstants.Public,
                PostPrivacyConstants.Private,
                PostPrivacyConstants.Custom
            };

            if (!validPrivacy.Contains(Privacy))
                yield return new ValidationResult("Privacy is invalid", [nameof(Privacy)]);
        }
        
        // Validate Date
        if (Date.HasValue && Date.Value > DateTime.UtcNow)
            yield return new ValidationResult("Date cannot be in the future", [nameof(Date)]);
    }
}