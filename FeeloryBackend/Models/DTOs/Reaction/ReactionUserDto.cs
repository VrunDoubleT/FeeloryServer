using FeeloryBackend.Models.DTOs.User;

namespace FeeloryBackend.Models.DTOs.Reaction;

public class ReactionUserDto
{
    public UserSummaryDto User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}