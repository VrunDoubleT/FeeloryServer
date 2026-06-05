namespace FeeloryBackend.Models.DTOs.User
{
    public class UserProfileDto
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? AvatarUrl { get; set; }
        public string FriendStatus { get; set; } = "none"; // none, pending, friend
    }
}
