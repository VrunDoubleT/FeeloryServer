using System.Text.Json;

namespace FeeloryBackend.Helpers;

public static class JsonOptionsHelper
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };
}