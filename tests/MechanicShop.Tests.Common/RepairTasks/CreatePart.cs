
using MechanicShop.Domain.Common.Results;

public static class PartFactory
{
    public static Result<Part> CreatePart(
        Guid? id = null, 
        string? name = null,
        decimal? cost = null, 
        int? quantity = null)
    {
        return Part.Create(
            id ?? Guid.NewGuid(),
            name ?? "Brake Pad",
            cost ?? 100,
            quantity ?? 2);
    }
}