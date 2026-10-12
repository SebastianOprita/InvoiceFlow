using InvoiceFlow.BuildingBlocks.Authorization;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[TenantScoped]
[Route("api/{tenantId:guid}/roles")]
public class RolesController(IMediator mediator) : ApiController
{
}
