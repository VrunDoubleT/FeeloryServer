namespace FeeloryBackend.Models.DTOs.Commons;

public class CursorPaginationRequest
{
    public string? Cursor { get; set; }

    public int PageSize { get; set; } = 10;
}