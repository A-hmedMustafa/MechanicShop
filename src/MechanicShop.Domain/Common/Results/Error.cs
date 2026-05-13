namespace MechanicShop.Domain.Common.Results;

public readonly record struct Error
{
    public string Code {get;}
    public string Description {get;}
    public ErrorKind Kind {get;}

    private Error(string code,string description, ErrorKind kind)
    {
        Code = code;
        Description = description;
        Kind = kind;
    }

    public static Error Failure(string code = nameof(Failure), 
        string description = "General Failure") => new(code, description, ErrorKind.Failure);
        
    public static Error Unexpected(string code = nameof(Unexpected), 
        string description = "Unexpected error occurred") => new(code, description, ErrorKind.Unexpected);
        
    public static Error Validation(string code = nameof(Validation), 
        string description = "Validation error occurred") => new(code, description, ErrorKind.Validation);
        
    public static Error Conflict(string code = nameof(Conflict), 
        string description = "Conflict error occurred") => new(code, description, ErrorKind.Conflict);
        
    public static Error NotFound(string code = nameof(NotFound), 
        string description = "Resource not found") => new(code, description, ErrorKind.NotFound);
        
    public static Error Unauthorized(string code = nameof(Unauthorized), 
        string description = "Unauthorized access") => new(code, description, ErrorKind.Unauthorized);
        
    public static Error Forbidden(string code = nameof(Forbidden), 
        string description = "Access forbidden") => new(code, description, ErrorKind.Forbidden);
}