namespace ByteBite.Application.DTOs.Common;

public class ApiResponse
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string? Detail { get; set; }

    public static ApiResponse Success(object? data, string message = "Success")
        => new() { Code = 200, Message = message, Data = data };

    public static ApiResponse Fail(int code, string message, string? detail = null)
        => new() { Code = code, Message = message, Data = null, Detail = detail };
}