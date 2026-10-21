using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace InvoiceFlow.Products.ArchitectureTests;

public sealed class ApplicationConventionTests
{
    [Fact]
    public void RequestHandlers_ShouldFollowNamingConvention()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() ==
                        typeof(IRequestHandler<,>))
                .Select(i => new
                {
                    Handler = type,
                    Request = i.GenericTypeArguments[0]
                }))
            .Where(x =>
                x.Handler.Name != $"{x.Request.Name}Handler")
            .Select(x =>
                $"{x.Handler.Name} should be " +
                $"{x.Request.Name}Handler")
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void RequestHandlers_ShouldBeInternal()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() ==
                        typeof(IRequestHandler<,>)) &&
                t.IsPublic)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Application_ShouldNotDependOnEntityFramework()
    {
        var result = Types
            .InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotDependOnAspNetCore()
    {
        var result = Types
            .InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void CommandHandlers_ShouldNotDependOnApi()
    {
        var result = Types
            .InAssembly(ArchitectureAssemblies.Application)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .ShouldNot()
            .HaveDependencyOn("InvoiceFlow.Invoices.Api")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Theory]
    [InlineData("Command")]
    [InlineData("Query")]
    public void Requests_ShouldBeSealedRecords(string suffix)
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.EndsWith(suffix))
            .Where(t =>
                !t.IsSealed ||
                t.GetMethod(
                    "<Clone>$",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic) is null)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Command")]
    [InlineData("Query")]
    public void Requests_ShouldImplementMediatR(
        string suffix)
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.EndsWith(suffix))
            .Where(t =>
                !t.GetInterfaces().Any(i =>
                    i == typeof(IRequest) ||
                    (i.IsGenericType &&
                     i.GetGenericTypeDefinition() ==
                        typeof(IRequest<>))))
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Requests_ShouldHaveExactlyOneHandler()
    {
        var assembly = ArchitectureAssemblies.Application;

        var requests = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                (t.Name.EndsWith("Command") ||
                 t.Name.EndsWith("Query")))
            .ToArray();

        var handlers = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    (i.GetGenericTypeDefinition() ==
                        typeof(IRequestHandler<,>) ||
                     i.GetGenericTypeDefinition() ==
                        typeof(IRequestHandler<>)))
                .Select(i => i.GenericTypeArguments[0]))
            .ToArray();

        var violations = requests
            .Where(request =>
                handlers.Count(h => h == request) != 1)
            .Select(request =>
                $"{request.Name} must have exactly one handler")
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Responses_ShouldBeSealedRecords()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.EndsWith("Response"))
            .Where(t =>
                !t.IsSealed ||
                t.GetMethod(
                    "<Clone>$",
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic) is null)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Responses_ShouldNotHavePublicSetters()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.EndsWith("Response"))
            .SelectMany(type =>
                type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(property =>
                        property.SetMethod?.IsPublic == true &&
                        !property.SetMethod.ReturnParameter
                            .GetRequiredCustomModifiers()
                            .Contains(typeof(
                                System.Runtime.CompilerServices.IsExternalInit)))
                    .Select(property =>
                        $"{type.Name}.{property.Name}"))
            .ToArray();

        violations.Should().BeEmpty();
    }
}
