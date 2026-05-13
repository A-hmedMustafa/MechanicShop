using MechanicShop.Domain.Emloyees;
using MechanicShop.Domain.Identity;
using MechanicShop.Tests.Common.Employees;
using Xunit;

namespace MechanicShop.Domain.UnitTests.Employees;

public class EmployeeTests
{
    [Fact]
    public void CreateEmployee_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string firstName = "John";
        const string lastName = "Doe";
        const Role role = Role.Labor;

        var employeeCreationResult = EmployeeFactory.CreateEmployee(
            id: id,
            firstName: firstName,
            lastName: lastName,
            role: role
        );

        var newEmployee = employeeCreationResult.Value;

        Assert.True(employeeCreationResult.IsSuccess);
        Assert.NotNull(newEmployee);
        Assert.IsType<Employee>(newEmployee);
        Assert.Equal(id, newEmployee.Id);
        Assert.Equal(firstName, newEmployee.FirstName);
        Assert.Equal(lastName, newEmployee.LastName);
        Assert.Equal(role, newEmployee.Role);
        Assert.Equal("John Doe", newEmployee.FullName);
    }

     [Fact]
    public void CreateEmployee_ShouldFail_WithEmptyId()
    {
        var employeeCreationResult = Employee.Create(Guid.Empty, "John", "Doe", Role.Manager);

        Assert.True(employeeCreationResult.IsError);
        Assert.Equal(EmployeeErrors.IdRequired.Code, employeeCreationResult.TopError.Code);
        Assert.Equal(EmployeeErrors.IdRequired.Description, employeeCreationResult.TopError.Description);
    }

    [Fact]
    public void CreateEmployee_ShouldFail_WithEmptyFirstName()
    {
        var employeeCreationResult = Employee.Create(Guid.NewGuid(), " ", "Doe", Role.Manager);

        Assert.True(employeeCreationResult.IsError);
        Assert.Equal(EmployeeErrors.FirstNameRequired.Code, employeeCreationResult.TopError.Code);
        Assert.Equal(EmployeeErrors.FirstNameRequired.Description, employeeCreationResult.TopError.Description);
    }

    [Fact]
    public void CreateEmployee_ShouldFail_WithEmptyLastName()
    {
        var employeeCreationResult = Employee.Create(Guid.NewGuid(), "John", " ", Role.Manager);

        Assert.True(employeeCreationResult.IsError);
        Assert.Equal(EmployeeErrors.LastNameRequired.Code, employeeCreationResult.TopError.Code);
        Assert.Equal(EmployeeErrors.LastNameRequired.Description, employeeCreationResult.TopError.Description);
    }

    [Fact]
    public void CreateEmployee_ShouldFail_WithInvalidRole()
    {
        var employeeCreationResult = Employee.Create(Guid.NewGuid(), "John", "Doe", (Role)999);

        Assert.True(employeeCreationResult.IsError);
        Assert.Equal(EmployeeErrors.RoleInvalid.Code, employeeCreationResult.TopError.Code);
        Assert.Equal(EmployeeErrors.RoleInvalid.Description, employeeCreationResult.TopError.Description);
    }
}