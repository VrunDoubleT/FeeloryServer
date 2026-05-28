using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Post;

public class GetFriendFeedRequestDto : IValidatableObject
{
    public string? Cursor { get; set; }
    public int Limit { get; set; } = 10;
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Limit <= 0)
            yield return new ValidationResult("Limit must be greater than 0", [nameof(Limit)]);
    }
}