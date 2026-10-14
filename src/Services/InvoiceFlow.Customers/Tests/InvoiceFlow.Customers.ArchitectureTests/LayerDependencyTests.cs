using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace InvoiceFlow.Customers.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string Domain =
        "InvoiceFlow.Customers.Domain";

    private const string Application =
        "InvoiceFlow.Customers.Application";

    private const string Infrastructure =
        "InvoiceFlow.Customers.Infrastructure";

    private const string Api =
        "InvoiceFlow.Customers.Api";

    [Fact]
    public void Domain_ShouldNotDependOnOuterLayers()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOnAny(Application, Infrastructure, Api)
            .GetResult();

        AssertArchitecture(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnOuterLayers()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOnAny(Infrastructure, Api)
            .GetResult();

        AssertArchitecture(result);
    }

    [Fact]
    public void Infrastructure_ShouldNotDependOnApi()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Infrastructure)
            .ShouldNot()
            .HaveDependencyOn(Api)
            .GetResult();

        AssertArchitecture(result);
    }

    [Fact]
    public void Domain_ShouldNotDependOnAuthorization()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Domain)
            .ShouldNot()
            .HaveDependencyOn("InvoiceFlow.BuildingBlocks.Authorization")
            .GetResult();

        AssertArchitecture(result);
    }

    [Fact]
    public void Application_ShouldNotDependOnAuthorization()
    {
        var result = Types.InAssembly(ArchitectureAssemblies.Application)
            .ShouldNot()
            .HaveDependencyOn("InvoiceFlow.BuildingBlocks.Authorization")
            .GetResult();

        AssertArchitecture(result);
    }

    private static void AssertArchitecture(NetArchTest.Rules.TestResult result)
    {
        var violations = result.FailingTypes?
            .Select(t => t.FullName)
            .ToArray() ?? [];

        result.IsSuccessful.Should().BeTrue(
            "architecture violations: {0}",
            string.Join(", ", violations));
    }
}
