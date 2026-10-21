using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace InvoiceFlow.Identity.ArchitectureTests;

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
                "InvoiceFlow.Identity.Infrastructure")
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
    public void ControllerActions_ShouldDeclareAuthorization()
    {
        var violations = ArchitectureAssemblies.Api
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                typeof(ControllerBase).IsAssignableFrom(t))
            .SelectMany(controller =>
                controller.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly)
                .Where(method =>
                    method.GetCustomAttributes<HttpMethodAttribute>(true)
                        .Any())
                .Select(method => new
                {
                    Controller = controller,
                    Action = method
                }))
            .Where(x =>
            {
                var hasAuthorization =
                    x.Controller.GetCustomAttributes(true)
                        .OfType<IAuthorizeData>().Any() ||
                    x.Action.GetCustomAttributes(true)
                        .OfType<IAuthorizeData>().Any();

                var allowsAnonymous =
                    x.Controller.GetCustomAttributes(true)
                        .OfType<IAllowAnonymous>().Any() ||
                    x.Action.GetCustomAttributes(true)
                        .OfType<IAllowAnonymous>().Any();

                return !hasAuthorization && !allowsAnonymous;
            })
            .Select(x =>
                $"{x.Controller.Name}.{x.Action.Name}")
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Theory]
    [InlineData("InvoiceFlow.Customers.")]
    [InlineData("InvoiceFlow.Invoices.")]
    [InlineData("InvoiceFlow.Notifications.")]
    [InlineData("InvoiceFlow.Payments.")]
    [InlineData("InvoiceFlow.Products.")]
    [InlineData("InvoiceFlow.Reminder.")]
    [InlineData("InvoiceFlow.Reporting.")]
    public void Identity_ShouldNotReferenceOtherServiceAssemblies(string forbiddenPrefix)
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
