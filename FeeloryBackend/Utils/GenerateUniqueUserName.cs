using System.Globalization;
using System.Text;
using FeeloryBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace FeeloryBackend.Utils;

public class GenerateUniqueUserName
{
    private readonly AppDbContext _dbContext;

    public GenerateUniqueUserName(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateUniqueUsernameAsync(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            displayName = "user";
        }
        
        string temp = displayName.Replace("đ", "d").Replace("Đ", "D");
        
        string normalized = temp.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c))
            {
                sb.Append(char.ToLowerInvariant(c));
            }
        }

        string baseUsername = sb.ToString();
        if (string.IsNullOrEmpty(baseUsername))
        {
            baseUsername = "user";
        }

        var existingUsernames = await _dbContext.Users
            .Where(u => u.Username.StartsWith(baseUsername))
            .Select(u => u.Username)
            .ToListAsync();

        string username = baseUsername;
        int counter = 1;
        
        while (existingUsernames.Contains(username))
        {
            username = $"{baseUsername}{counter}";
            counter++;
        }

        return username;
    }  
}