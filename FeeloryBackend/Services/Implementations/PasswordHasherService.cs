using FeeloryBackend.Services.Interfaces;

namespace FeeloryBackend.Services.Implementations;

public class PasswordHasherService : IPasswordHasherService
{
    public string HashPassword(string password)
    {
        // Hash password using BCrypt with salt
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        // Verify password against stored hash
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}