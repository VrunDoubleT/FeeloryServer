using System.ComponentModel.DataAnnotations;
using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.DTOs.Post;

public class GetMyPostsRequestDto : IValidatableObject
{
    public DateTime? Date { get; set; }
    public string? Privacy { get; set; }
    public string? Cursor { get; set; }
    public int Limit { get; set; } = 10;
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

        // Validate Limit
        if (Limit <= 0)
            yield return new ValidationResult("Limit must be greater than 0", [nameof(Limit)]);
        
        // Validate Date
        if (Date.HasValue && Date.Value > DateTime.UtcNow)
            yield return new ValidationResult("Date cannot be in the future", [nameof(Date)]);
    }
}