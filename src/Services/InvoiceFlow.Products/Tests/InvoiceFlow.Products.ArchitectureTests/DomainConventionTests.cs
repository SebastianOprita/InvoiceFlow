using FluentAssertions;
using NetArchTest.Rules;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;

namespace InvoiceFlow.Products.ArchitectureTests;

public sealed class DomainConventionTests
{
    private static IEnumerable<Type> GetDomainClasses()
    {
        return ArchitectureAssemblies.Domain
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                !t.IsDefined(typeof(CompilerGeneratedAttribute), false));
    }

    [Fact]
    public void DomainClasses_ShouldBeSealed()
    {
        var violations = GetDomainClasses()
            .Where(t => !t.IsSealed)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void DomainClasses_ShouldNotHavePublicConstructors()
    {
        var violations = GetDomainClasses()
            .Where(t => t.GetConstructors(
                BindingFlags.Public |
                BindingFlags.Instance).Length > 0)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void DomainClasses_ShouldExposePublicStaticCreateMethod()
    {
        var violations = GetDomainClasses()
            .Where(t => !t.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.Static |
                    BindingFlags.DeclaredOnly)
                .Any(m => m.Name == "Create"))
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
