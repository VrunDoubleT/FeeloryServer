using FeeloryBackend.Models.Entities;

namespace FeeloryBackend.Services.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}