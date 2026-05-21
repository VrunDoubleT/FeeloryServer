using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Friend;

public class SendFriendRequestDto : IValidatableObject
{
    public Guid ReceiverId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (ReceiverId == Guid.Empty)
            yield return new ValidationResult("The receiverId is invalid", [nameof(ReceiverId)]);
    }
}