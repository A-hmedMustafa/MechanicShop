using MechanicShop.Application.Features.Labor.Dtos;
using MechanicShop.Domain.Emloyees;

namespace MechanicShop.Application.Features.Labors.Mappers;

public static class LaborMapper
{
    public static LaborDto ToDto(this Employee employee)
    {
        return new LaborDto { LaborId = employee.Id, 
        Name = employee.FullName };
    }

    public static List<LaborDto> ToDtos(this IEnumerable<Employee> entities)
    {
        return [.. entities.Select(l => l.ToDto())];
    }
}