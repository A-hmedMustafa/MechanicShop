using MechanicShop.Client.Models;

namespace MechanicShop.Client.Extensions;

public static class ProblemDetailsExtensions
{
    private const string DefaultErrorMessage = "An unknown error occurred.";

    public static string TopError(this ProblemDetails? problem)
    {
        return problem switch
        {
            null => DefaultErrorMessage,
            _ => problem.Title!
        };
    }
}