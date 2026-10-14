using FluentAssertions;
using FluentValidation;
using Xunit;

namespace InvoiceFlow.Identity.ArchitectureTests;

public sealed class ValidatorConventionTests
{
    [Fact]
    public void Validators_ShouldBeInternalAndSealed()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                IsValidator(t))
            .Where(t =>
                !t.IsNotPublic ||
                !t.IsSealed)
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    private static bool IsValidator(Type type)
    {
        return type.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IValidator<>));
    }

    [Fact]
    public void Validators_ShouldInheritAbstractValidator()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidator<>)))
            .Where(t => !InheritsAbstractValidator(t))
            .Select(t => t.FullName)
            .ToArray();

        violations.Should().BeEmpty();
    }

    private static bool InheritsAbstractValidator(Type type)
    {
        for (var current = type.BaseType;
             current is not null;
             current = current.BaseType)
        {
            if (current.IsGenericType &&
                current.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            {
                return true;
            }
        }

        return false;
    }

    [Fact]
    public void Validators_ShouldValidateApplicationTypes()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new
                {
                    Validator = type,
                    ValidatedType = i.GenericTypeArguments[0]
                }))
            .Where(x =>
                x.ValidatedType.Assembly != ArchitectureAssemblies.Application)
            .Select(x =>
                $"{x.Validator.Name} validates " +
                $"{x.ValidatedType.FullName}")
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Requests_ShouldHaveAtMostOneValidator()
    {
        var validators = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new
                {
                    Validator = type,
                    Request = i.GenericTypeArguments[0]
                }))
            .ToArray();

        var violations = validators
            .GroupBy(x => x.Request)
            .Where(group => group.Count() > 1)
            .Select(group =>
                $"{group.Key.Name} has multiple validators: " +
                string.Join(", ", group.Select(x => x.Validator.Name)))
            .ToArray();

        violations.Should().BeEmpty();
    }

    [Fact]
    public void Validators_ShouldFollowNamingConvention()
    {
        var violations = ArchitectureAssemblies.Application
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(type => type.GetInterfaces()
                .Where(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidator<>))
                .Select(i => new
                {
                    Validator = type,
                    ValidatedType = i.GenericTypeArguments[0]
                }))
            .Where(x =>
                x.Validator.Name != $"{x.ValidatedType.Name}Validator")
            .Select(x =>
                $"{x.Validator.Name} should be named " +
                $"{x.ValidatedType.Name}Validator")
            .ToArray();

        violations.Should().BeEmpty();
    }
}
