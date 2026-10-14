using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace InvoiceFlow.Customers.ArchitectureTests;

public sealed class DomainConventionTests
{
    [Fact]
    public void DomainClasses_ShouldBeSealed()
    {
        var violations = ArchitectureAssemblies.Domain
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                !t.IsSealed)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Theory]
    [InlineData("System.IdentityModel.Tokens.Jwt")]
    [InlineData("Microsoft.IdentityModel")]
    [InlineData("Microsoft.AspNetCore.Authentication")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    public void Domain_ShouldNotDependOnLibraries(
    string dependency)
    {
        var result = Types
            .InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(dependency)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Theory]
    [InlineData("MediatR")]
    [InlineData("MassTransit")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.AspNetCore")]
    public void Domain_ShouldNotDependOnFrameworks(
    string dependency)
    {
        var result = Types
            .InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn(dependency)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
