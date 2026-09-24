using InvoiceFlow.BuildingBlocks.Domain;
using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace InvoiceFlow.BuildingBlocks.Application;

public static class ErrorCatalogue
{
    public static IReadOnlyCollection<ErrorDefinitionResponse> GetDomainErrors(Type domainErrorsType)
    {
        return GetNestedStaticProperties(domainErrorsType)
            .Where(property => property.PropertyType == typeof(DomainError))
            .Select(property =>
            {
                var error = (DomainError)property.GetValue(null)!;

                return new ErrorDefinitionResponse(
                    Code: error.ErrorCode,
                    Message: error.ErrorMessage,
                    Source: CreateSource("domain", property.DeclaringType),
                    SuggestedStatus: StatusCodes.Status400BadRequest);
            })
            .OrderBy(error => error.Code)
            .ToArray();
    }

    public static IReadOnlyCollection<ErrorDefinitionResponse> GetApplicationErrors(Type applicationErrorsType)
    {
        return GetNestedStaticProperties(applicationErrorsType)
            .Where(property => property.PropertyType == typeof(ApplicationError))
            .Select(property =>
            {
                var error = (ApplicationError)property.GetValue(null)!;

                return new ErrorDefinitionResponse(
                    Code: error.Code,
                    Message: error.Message,
                    Source: CreateSource("application", property.DeclaringType),
                    SuggestedStatus: MapStatusCode(error.Type));
            })
            .OrderBy(error => error.Code)
            .ToArray();
    }

    private static IEnumerable<PropertyInfo> GetNestedStaticProperties(
        Type containerType)
    {
        const BindingFlags propertyFlags =
            BindingFlags.Public |
            BindingFlags.Static;

        return containerType
            .GetNestedTypes(BindingFlags.Public)
            .SelectMany(type => type.GetProperties(propertyFlags));
    }

    private static string CreateSource(
        string layer,
        Type? declaringType)
    {
        var groupName = declaringType?.Name.ToLowerInvariant() ?? "unknown";

        return $"{layer}.{groupName}";
    }

    private static int MapStatusCode(ApplicationErrorType type)
    {
        return type switch
        {
            ApplicationErrorType.Validation => StatusCodes.Status400BadRequest,
            ApplicationErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ApplicationErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ApplicationErrorType.NotFound => StatusCodes.Status404NotFound,
            ApplicationErrorType.Conflict => StatusCodes.Status409Conflict,
            ApplicationErrorType.Internal => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}
