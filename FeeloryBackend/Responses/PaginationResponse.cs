namespace FeeloryBackend.Responses;

public class PaginationResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; }

    public IEnumerable<T> Data { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public int TotalPages { get; set; }

    public PaginationResponse(
        IEnumerable<T> data,
        int pageNumber,
        int pageSize,
        int totalRecords,
        string message = "Success")
    {
        Success = true;
        Message = message;

        Data = data;

        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;

        TotalPages = (int)Math.Ceiling(
            totalRecords / (double)pageSize
        );
    }
}