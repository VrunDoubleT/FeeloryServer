using System.Security.Cryptography;
using System.Text;

namespace FeeloryBackend.Helpers;

public static class HashedToken
{
    public static string GetHashedKey(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return $"refresh_token:{Convert.ToHexString(hashBytes).ToLowerInvariant()}";
    }
    
    public static string GetVersionKey(Guid userId)
        => $"token_version:{userId}";
}