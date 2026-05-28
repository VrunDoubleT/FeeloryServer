using System.ComponentModel.DataAnnotations;
using FeeloryBackend.Constants;

namespace FeeloryBackend.Models.DTOs.Post;

public class CreatePostRequestDto : IValidatableObject
{
    public IFormFile Image { get; set; } = null!;
    public string? Description { get; set; }
    public Guid MoodEmoteId { get; set; }
    public string Privacy { get; set; } = null!;
    public List<Guid>? AllowedUserIds { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        // Validate Image
        if (Image == null)
        {
            yield return new ValidationResult("Image is required", [nameof(Image)]);
        }
        else
        {
            // Check file extension
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(Image.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
                yield return new ValidationResult("Only .jpg, .jpeg, .png, .webp files are allowed", [nameof(Image)]);

            // Check file size (5MB)
            const long maxFileSize = 5 * 1024 * 1024;
            if (Image.Length > maxFileSize)
                yield return new ValidationResult("Image size must not exceed 5MB", [nameof(Image)]);
        }

        // Validate Description
        if (string.IsNullOrWhiteSpace(Description))
            yield return new ValidationResult("Description is required", [nameof(Description)]);
        
        // Validate MoodEmoteId
        if (MoodEmoteId == Guid.Empty)
            yield return new ValidationResult("MoodEmoteId is invalid", [nameof(MoodEmoteId)]);
        
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
            if (AllowedUserIds == null || AllowedUserIds.Count == 0)
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
            if (AllowedUserIds != null && AllowedUserIds.Count > 0)
            {
                yield return new ValidationResult("AllowedUserIds is only allowed when privacy is CUSTOM", [nameof(AllowedUserIds)]);
            }
        }
    }
}