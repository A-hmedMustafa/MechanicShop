using MechanicShop.Domain.Common.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace MechanicShop.Api.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    protected ActionResult Problem(List<Error> errors)
    {
        if(errors.Count is 0)
            return Problem();

        if(errors.All(error => error.Kind == ErrorKind.Validation))
            return ValidationProblem(errors);            

        return Problem(errors[0]);    
    }

    private ObjectResult Problem(Error error)
    {
        var statusCode = error.Kind switch
        {
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };
        return Problem(statusCode:statusCode, title: error.Description);
    }

    private ActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        errors.ForEach(error => modelStateDictionary.AddModelError(error.Code, error.Description));

        return ValidationProblem(modelStateDictionary);
    }
}