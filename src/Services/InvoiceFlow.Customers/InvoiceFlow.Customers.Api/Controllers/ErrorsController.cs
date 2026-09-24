using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Customers.Application;
using InvoiceFlow.Customers.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Customers.Api.Controllers;

[ApiController]
[Route("api/errors")]
public sealed class ErrorsController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<ErrorCatalogueResponse>(StatusCodes.Status200OK)]
    public ActionResult<ErrorCatalogueResponse> Get()
    {
        var response = ErrorCatalogueResponse.GetErrorCatalogueResponse(
            typeof(DomainErrors),
            typeof(ApplicationErrors));

        return Ok(response);
    }
}
