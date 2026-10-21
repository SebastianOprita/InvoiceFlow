using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetArchTest.Rules;
using Xunit;

namespace InvoiceFlow.Products.ArchitectureTests;

public sealed class ApiConventionTests
{
    [Fact]
    public void Controllers_ShouldNotInjectRepositories()
    {
        var violations = ArchitectureAssemblies.Api
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(controller =>
                controller.GetConstructors()
                    .SelectMany(ctor => ctor.GetParameters())
                    .Where(parameter =>
                        parameter.ParameterType.Name
                            .EndsWith("Repository"))
                    .Select(parameter =>
                        $"{controller.Name} injects " +
                        $"{parameter.ParameterType.Name}"))
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Controllers_ShouldNotDependOnInfrastructure()
    {
        var result = Types
            .InAssembly(ArchitectureAssemblies.Api)
            .That()
            .Inherit(typeof(ControllerBase))
            .ShouldNot()
            .HaveDependencyOn(
                "InvoiceFlow.Invoices.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Controllers_ShouldBePublicAndFollowNamingConvention()
    {
        var violations = ArchitectureAssemblies.Api
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(t))
            .Where(t =>
                !t.IsPublic ||
                !t.Name.EndsWith("Controller"))
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Controllers_ShouldRequireAuthorizationOrAllowAnonymous()
    {
        var violations = ArchitectureAssemblies.Api
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(t))
            .Where(t =>
                !t.IsDefined(typeof(AuthorizeAttribute), inherit: true) &&
                !t.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Theory]
    [InlineData("InvoiceFlow.Customers.")]
    [InlineData("InvoiceFlow.Identity.")]
    [InlineData("InvoiceFlow.Invoices.")]
    [InlineData("InvoiceFlow.Notifications.")]
    [InlineData("InvoiceFlow.Payments.")]
    [InlineData("InvoiceFlow.Reminder.")]
    [InlineData("InvoiceFlow.Reporting.")]
    public void Products_ShouldNotReferenceOtherServiceAssemblies(string forbiddenPrefix)
    {
        var assemblies = new[]
        {
            ArchitectureAssemblies.Domain,
            ArchitectureAssemblies.Application,
            ArchitectureAssemblies.Infrastructure,
            ArchitectureAssemblies.Api
        };

        var violations = assemblies
            .SelectMany(assembly =>
                assembly.GetReferencedAssemblies()
                    .Where(reference =>
                        reference.Name?.StartsWith(
                            forbiddenPrefix,
                            StringComparison.Ordinal) == true)
                    .Select(reference =>
                        $"{assembly.GetName().Name} -> {reference.Name}"))
            .ToArray();

        violations.Should().BeEmpty();
    }
}
