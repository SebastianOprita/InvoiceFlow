using FluentAssertions;
using System.Reflection;
using Xunit;

namespace InvoiceFlow.Customers.ArchitectureTests;

public sealed class AssemblyMetadataTests
{
    public static TheoryData<Assembly> Assemblies => new()
    {
        ArchitectureAssemblies.Domain,
        ArchitectureAssemblies.Application,
        ArchitectureAssemblies.Infrastructure,
        ArchitectureAssemblies.Api
    };

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void Assembly_ShouldHaveCorrectMetadata(Assembly assembly)
    {
        var name = assembly.GetName();

        var title = assembly
            .GetCustomAttribute<AssemblyTitleAttribute>()?.Title;

        var company = assembly
            .GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;

        var product = assembly
            .GetCustomAttribute<AssemblyProductAttribute>()?.Product;

        var copyright = assembly
            .GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright;

        var version = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        name.Name.Should().StartWith("InvoiceFlow.");

        title.Should().Be(name.Name);
        company.Should().Be("SebastianOprita");
        product.Should().Be("InvoiceFlow");

        copyright.Should().Be(
            "Copyright © Sebastian Oprita. All rights reserved.");

        name.Version.Should().Be(new Version(1, 0, 0, 0));

        version.Should().StartWith("1.0.0");
    }

    [Theory]
    [MemberData(nameof(Assemblies))]
    public void Assembly_ShouldFollowNamingConvention(Assembly assembly)
    {
        var assemblyName = assembly.GetName().Name;

        assemblyName.Should()
            .MatchRegex(
                @"^InvoiceFlow\.[A-Za-z]+\.(Domain|Application|Infrastructure|Api)$");
    }
}
