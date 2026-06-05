using System.ComponentModel.DataAnnotations;

namespace FeeloryBackend.Models.DTOs.Reaction;

public class AddPostReactionRequestDto
{
    [Required] public Guid PostId { get; set; }

    [Required] public Guid EmoteId { get; set; }
}