using FluentValidation;
using FluentValidation.Results;
using MechanicShop.Application.Common.Behaviors;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Tests.Common.WorkOrders;
using MediatR;
using NSubstitute;
using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly IValidator<CreateWorkOrderCommand> _validator = Substitute.For<IValidator<CreateWorkOrderCommand>>();
    private readonly RequestHandlerDelegate<Result<WorkOrderDto>> _next = Substitute.For<RequestHandlerDelegate<Result<WorkOrderDto>>>();
    private readonly ValidationBehavior<CreateWorkOrderCommand, Result<WorkOrderDto>> _sut;

    public ValidationBehaviorTests()
    {
        _sut = new(_validator);
    }

    [Fact]
    public async Task Handle_WhenValidationSucceed_ShouldInvokeNextBehavior()
    {
        var workOrderCreationCommand = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
        var newWorkOrder = WorkOrderFactory.CreateWorkOrder().Value.ToDto();
        var cancellationToken = CancellationToken.None;

        _validator.ValidateAsync(workOrderCreationCommand, cancellationToken)
            .Returns(new ValidationResult());

        _next.Invoke(cancellationToken).Returns(newWorkOrder);


        var result = await _sut.Handle(workOrderCreationCommand, _next, cancellationToken);

        Assert.True(result.IsSuccess);  
        Assert.Equal(newWorkOrder, result.Value);  

    }

    [Fact]
    public async Task Handle_WhenValidationFail_ShouldReturnErrors()
    {
        var workOrderCreationCommand = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
        var cancellationToken = CancellationToken.None;
        List<ValidationFailure> validationFailures = [new(propertyName: "prop1", errorMessage:"prop1 is invalid")];
        _validator.ValidateAsync(workOrderCreationCommand, cancellationToken)
            .Returns(new ValidationResult(validationFailures));

        var result = await _sut.Handle(workOrderCreationCommand, _next, cancellationToken);

        Assert.True(result.IsError);
        Assert.Equal("prop1", result.TopError.Code);
        Assert.Equal("prop1 is invalid", result.TopError.Description);
    }

    [Fact]
    public async Task Handle_WhenNoValidationRequired_ShouldInvokeNextBehavior()
    {
        var workOrderCreationCommand = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
        var rawValidationBehavior = new ValidationBehavior<CreateWorkOrderCommand, Result<WorkOrderDto>>();
        var newWorkOrder = WorkOrderFactory.CreateWorkOrder().Value.ToDto();
        var cancellationToken = CancellationToken.None;
        _next.Invoke(cancellationToken).Returns(newWorkOrder);

        var result = await rawValidationBehavior.Handle(workOrderCreationCommand, _next, cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(newWorkOrder, result.Value);
    }
}