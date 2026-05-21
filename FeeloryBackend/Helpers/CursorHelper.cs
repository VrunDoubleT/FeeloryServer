using System.Text;

namespace FeeloryBackend.Helpers;

public static class CursorHelper
{
    public static string Encode(DateTime createdAtUtc, Guid id)
    {
        var raw = $"{createdAtUtc:o}|{id}";

        return Convert.ToBase64String(
            Encoding.UTF8.GetBytes(raw)
        );
    }

    public static (DateTime createdAtUtc, Guid id) Decode(string cursor)
    {
        var raw = Encoding.UTF8.GetString(
            Convert.FromBase64String(cursor)
        );

        var parts = raw.Split('|');

        return (
            DateTime.Parse(parts[0]),
            Guid.Parse(parts[1])
        );
    }
}