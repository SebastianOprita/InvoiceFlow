using FluentAssertions;
using Xunit;

namespace InvoiceFlow.Invoices.ArchitectureTests;

public sealed class InfrastructureConventionTests
{
    [Fact]
    public void Services_ShouldBeInternal()
    {
        var violations = ArchitectureAssemblies.Infrastructure
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.EndsWith("Service") &&
                t.IsPublic)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Repositories_ShouldBeInternal()
    {
        var violations = ArchitectureAssemblies.Infrastructure
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.EndsWith("Repository") &&
                t.IsPublic)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }
}
