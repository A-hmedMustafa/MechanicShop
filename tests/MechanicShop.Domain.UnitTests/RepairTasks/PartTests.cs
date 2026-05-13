using MechanicShop.Application.Common.Errors;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Domain.RepairTasks.Parts;
using Xunit;

namespace MechanicShop.Domain.UnitTests.RepairTasks;

public class PartTests
{
    [Fact]
    public void CreatePart_WithValidData_ShouldSucceed()
    {
        Guid id = Guid.NewGuid();
        const string name = "brakes";
        const decimal cost = 15.00m;
        const int quantity = 3;

        var partCreationResult = PartFactory.CreatePart(
            id:id,
            name:name,
            cost:cost,
            quantity:quantity
        );

        var newPart = partCreationResult.Value;

        Assert.True(partCreationResult.IsSuccess);
        Assert.NotNull(newPart);
        Assert.IsType<Part>(newPart);
        Assert.Equal(id, newPart.Id);
        Assert.Equal(name, newPart.Name);
        Assert.Equal(cost, newPart.Cost);
        Assert.Equal(quantity, newPart.Quantity);

    }

   [Fact]
    public void CreatePart_WhenInvalidName_ShouldFail()
    {
        var partCreationResult = PartFactory.CreatePart(name: " ");

        Assert.True(partCreationResult.IsError);
        Assert.Equal(PartErrors.NameRequired.Code, partCreationResult.TopError.Code);
    }
    
    [Fact]
    public void Create_WithInvalidCost_ShouldFail()
    {
        var partCreationResult = PartFactory.CreatePart(cost: 0);

        Assert.True(partCreationResult.IsError);

        Assert.Equal(PartErrors.CostInvalid.Code, partCreationResult.TopError.Code);
    }

    [Fact]
    public void Create_WithInvalidQuantity_ShouldFail()
    {
        var partCreationResult = PartFactory.CreatePart(quantity: 0);

        Assert.True(partCreationResult.IsError);

        Assert.Equal(PartErrors.QuantityInvalid.Code, partCreationResult.TopError.Code);
    }

    [Fact]
    public void Update_WithValidData_ShouldSucceed()
    {
        var part = PartFactory.CreatePart().Value;

        const string name = "Brake Disc";
        const decimal cost = 200m;
        const int quantity = 3;

        var partUpdateResult = part.Update(name, cost, quantity);

        Assert.True(partUpdateResult.IsSuccess);
        Assert.Equal(Result.Updated, partUpdateResult.Value);
        Assert.Equal(name, part.Name);
        Assert.Equal(cost, part.Cost);
        Assert.Equal(quantity, part.Quantity);
    }

    [Fact]
    public void Update_WithInvalidName_ShouldFail()
    {
        var part = PartFactory.CreatePart().Value;

        const string name = " ";
        const decimal cost = 200m;
        const int quantity = 3;

        var partUpdateResult = part.Update(name, cost, quantity);

        Assert.True(partUpdateResult.IsError);

        Assert.Equal(PartErrors.NameRequired.Code, partUpdateResult.TopError.Code);
    }

    [Fact]
    public void Update_WithInvalidCost_ShouldFail()
    {
        var part = PartFactory.CreatePart().Value;

        const string name = "Brake Disc";
        const decimal cost = 0m;
        const int quantity = 3;

        var partUpdateResult = part.Update(name, cost, quantity);

        Assert.True(partUpdateResult.IsError);

        Assert.Equal(PartErrors.CostInvalid.Code, partUpdateResult.TopError.Code);
    }

    [Fact]
    public void Update_WithInvalidQuantity_ShouldFail()
    {
        var part = PartFactory.CreatePart().Value;

        const string name = "Brake Disc";
        const decimal cost = 200m;
        const int quantity = 0;

        var partUpdateResult = part.Update(name, cost, quantity);

        Assert.True(partUpdateResult.IsError);

        Assert.Equal(PartErrors.QuantityInvalid.Code, partUpdateResult.TopError.Code);
    }
}