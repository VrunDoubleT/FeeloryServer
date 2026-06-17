using FeeloryBackend.Models.DTOs.Auth;
using FeeloryBackend.Models.DTOs.Emote;
using FeeloryBackend.Models.DTOs.Reaction;

namespace FeeloryBackend.Models.DTOs.Post;

public class PostDetailDto
{
    public PostDto Post { get; set; } = null!;
    public UserDto Owner { get; set; } = null!;
    public EmoteDto? Emote { get; set; }
    public List<ReactionGroupDto> Reactions { get; set; } = [];
}