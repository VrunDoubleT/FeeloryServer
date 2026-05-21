using System.Text.Json.Serialization;

namespace FeeloryBackend.Responses;

public class ApiErrorResponse
{
    public bool Success { get; set; }

    public string Message { get; set; }

    [JsonIgnore(Condition =  JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? Errors { get; set; }

    public ApiErrorResponse(
        string message,
        List<string>? errors = null)
    {
        Success = false;
        Message = message;
        Errors = errors;
    }
}