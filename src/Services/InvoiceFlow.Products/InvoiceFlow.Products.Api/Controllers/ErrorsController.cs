using System.Reflection;
using InvoiceFlow.BuildingBlocks.Api;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.BuildingBlocks.Domain;
using InvoiceFlow.Products.Application;
using InvoiceFlow.Products.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Products.Api.Controllers;

[ApiController]
[AllowAnonymous]
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
        return GetStaticFields(typeof(DomainErrors))
            .Where(field => field.FieldType == typeof(DomainError))
            .Select(field =>
            {
                var error = (DomainError)field.GetValue(null)!;

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
        return GetStaticFields(typeof(ApplicationErrors))
            .Where(field => field.FieldType == typeof(ApplicationError))
            .Select(field =>
            {
                var error = (ApplicationError)field.GetValue(null)!;

                return new ErrorDefinitionResponse(
                    Code: error.Code,
                    Message: error.Message,
                    Source: "application",
                    SuggestedStatus: MapStatusCode(error.Type));
            })
            .OrderBy(error => error.Code)
            .ToArray();
    }

    private static FieldInfo[] GetStaticFields(Type type)
    {
        return type.GetFields(
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
