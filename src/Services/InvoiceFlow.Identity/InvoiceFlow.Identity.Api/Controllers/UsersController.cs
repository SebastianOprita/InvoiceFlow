using InvoiceFlow.BuildingBlocks.Authorization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[Authorize]
[TenantScoped]
[Route("api/{tenantId:guid}/users")]
public class UsersController(IMediator mediator) : ApiController
{
}
