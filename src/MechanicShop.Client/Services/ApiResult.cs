using System.Collections.Generic;
using System.Linq;
namespace MechanicShop.Client.Services;

public class ApiResult<T>
{
    public bool IsSuccess {get; set; }
    public T? Data {get; set; }
    public string? ErrorMessage {get; set; }
    public string? ErrorDetails {get; set; }
    public int StatusCode {get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; } 
    public string? FirstErrorMessage =>
        ValidationErrors.SelectMany(e => e.Value).FirstOrDefault() ?? ErrorMessage;

    public static ApiResult<T> Success(T data)
    {
        return new ApiResult<T>
        {
            IsSuccess = true,
            Data = data
        };
    }    
    public static ApiResult<T> Failure(string? message, string? details = null, int statusCode = 0, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorDetails = details,
            StatusCode = statusCode,
            ValidationErrors = validationErrors
        };
    }    
}

public class ApiResult : ApiResult<object>
{
    public new static ApiResult Success()
    {
        return new ApiResult
        {
            IsSuccess = true
        };
    }    
    public new static ApiResult Failure(string? message, string? details = null, int statusCode = 0, Dictionary<string, string[]>? validationErrors = null)
    {
        return new ApiResult
        {
            IsSuccess = false,
            ErrorMessage = message,
            ErrorDetails = details,
            StatusCode = statusCode,
            ValidationErrors = validationErrors
        };
    }
}