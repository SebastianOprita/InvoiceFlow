using InvoiceFlow.BuildingBlocks.Authorization.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

[ApiController]
[Authorize]
[PlatformUserLogin]
[Route("api/platform/users")]
public class PlatformUsersController(IMediator mediator) : ApiController
{
}
