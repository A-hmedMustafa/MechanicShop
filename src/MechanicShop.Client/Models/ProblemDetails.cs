using System.Collections.Generic;

namespace MechanicShop.Client.Models;

public class ProblemDetails
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? Status { get; set; }
    public string Detail { get; set; } = string.Empty;
    public string Instance { get; set; } = string.Empty;

    public Dictionary<string, string[]> Errors { get; init; } = [];
    public Dictionary<string, object> Extensions { get; set; } = [];
}