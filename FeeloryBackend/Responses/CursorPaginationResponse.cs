namespace FeeloryBackend.Responses;

public class CursorPaginationResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; }

    public IEnumerable<T> Data { get; set; }

    // // Cursor for the next page  
    public string? NextCursor { get; set; }

    // Indicates whether more data exists
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