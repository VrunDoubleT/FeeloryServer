using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Friend;

public class AcceptFriendRequestDto : IValidatableObject
{
    public Guid RequestId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (RequestId == Guid.Empty)
            yield return new ValidationResult("The requestId is invalid", [nameof(RequestId)]);
    }
}