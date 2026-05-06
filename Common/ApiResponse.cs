namespace ClinicBooking.API.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success") => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Fail(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}

// Non-generic version for responses with no data (delete, logout, etc.)
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse OkNoData(string message = "Success") => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse FailNoData(string message, List<string>? errors = null) => new()
    {
        Success = false,
        Message = message,
        Errors = errors
    };
}
