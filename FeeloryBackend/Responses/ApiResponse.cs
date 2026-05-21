using System.Text.Json.Serialization;

namespace FeeloryBackend.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public string Message { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? Data { get; set; }

    public ApiResponse(
        T? data,
        string message = "Success")
    {
        Success = true;
        Message = message;
        Data = data;
    }
}