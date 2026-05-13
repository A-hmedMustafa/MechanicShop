using MechanicShop.Application.Features.Labors.Mappers;
using MechanicShop.Domain.Emloyees;
using MechanicShop.Tests.Common.Employees;
using Microsoft.VisualBasic;
using Xunit;

namespace MechanicShop.Application.UnitTests.Mappers;

public class LaborMapperTests
{
    [Fact]
    public void ToDto_ShouldMapCorrectly()
    {
        var labor = EmployeeFactory.CreateLabor().Value;

        var laborDto = labor.ToDto();

        Assert.NotNull(laborDto);
        Assert.Equal(labor.Id, laborDto.LaborId);
        Assert.Equal(labor.FullName, laborDto.Name);
    }
    [Fact]
    public void ToDtos_ShouldMapCorrectly()
    {
        var labor = EmployeeFactory.CreateLabor().Value;
        var labors = new List<Employee> {labor};

        var laborsDtos = labors.ToDtos();
        Assert.Single(laborsDtos);

        var laborDto = laborsDtos[0];
        Assert.NotNull(laborDto);
        Assert.Equal(labor.Id, laborDto.LaborId);
        Assert.Equal(labor.FullName, laborDto.Name);
    }

}