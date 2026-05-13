using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Extensions;

public static class ProblemExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if(errors.Count() == 0)
            return Results.Problem();

        if(errors.All(error => error.Kind == ErrorKind.Validation))
            return ValidationProblem(errors);

        return Problem(errors[0]);
    }

    private static IResult Problem(Error error)
    {
        var statusCode = error.Kind switch
        {
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    private static IResult ValidationProblem(List<Error> errors)
    {
        var errorsDictionary = errors.ToDictionary(e=> e.Code, e=> new [] {e.Description});
        var problemDetails = new ValidationProblemDetails(errorsDictionary)
        {
            Status = StatusCodes.Status400BadRequest
        };
        
        return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
    }
}