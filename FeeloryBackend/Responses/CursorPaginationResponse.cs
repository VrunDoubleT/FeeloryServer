namespace FeeloryBackend.Responses;

public class CursorPaginationResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; }

    public IEnumerable<T> Data { get; set; }

    // Cursor cho page tiếp theo
    public string? NextCursor { get; set; }

    // Có còn dữ liệu không
    public bool HasNextPage { get; set; }

    public CursorPaginationResponse(
        IEnumerable<T> data,
        string? nextCursor,
        bool hasNextPage,
        string message = "Success")
    {
        Success = true;
        Message = message;

        Data = data;

        NextCursor = nextCursor;
        HasNextPage = hasNextPage;
    }
}