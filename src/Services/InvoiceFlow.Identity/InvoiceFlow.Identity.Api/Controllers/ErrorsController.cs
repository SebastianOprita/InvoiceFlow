using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[Route("api/errors")]
public sealed class ErrorsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ErrorCatalogueResponse>(StatusCodes.Status200OK)]
    public ActionResult<ErrorCatalogueResponse> Get()
    {
        var response = new ErrorCatalogueResponse(
            DomainErrors: GetDomainErrors(),
            ApplicationErrors: GetApplicationErrors());

        return Ok(response);
    }

    private static ErrorDefinitionResponse[] GetDomainErrors()
    {
        return GetStaticProperties(typeof(DomainErrors))
            .Where(property => property.PropertyType == typeof(DomainError))
            .Select(property =>
            {
                var error = (DomainError)property.GetValue(null)!;

                return new ErrorDefinitionResponse(
                    Code: error.ErrorCode,
                    Message: error.ErrorMessage,
                    Source: "domain",
                    SuggestedStatus: StatusCodes.Status400BadRequest);
            })
            .OrderBy(error => error.Code)
            .ToArray();
    }

    private static ErrorDefinitionResponse[] GetApplicationErrors()
    {
        return GetStaticProperties(typeof(ApplicationErrors))
            .Where(property => property.PropertyType == typeof(ApplicationError))
            .Select(property =>
            {
                var error = (ApplicationError)property.GetValue(null)!;

                return new ErrorDefinitionResponse(
                    Code: error.Code,
                    Message: error.Message,
                    Source: "application",
                    SuggestedStatus: MapStatusCode(error.Type));
            })
            .OrderBy(error => error.Code)
            .ToArray();
    }

    private static PropertyInfo[] GetStaticProperties(Type type)
    {
        return type.GetProperties(
            BindingFlags.Public |
            BindingFlags.Static);
    }

    private static int MapStatusCode(ApplicationErrorType type) =>
        type switch
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
