using InvoiceFlow.BuildingBlocks.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api;

public record PermissionResponse(string Name, int Value);

[ApiController]
[Authorize]
[Route("api/permissions")]
public class PermissionsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll() =>
        Ok(Enum.GetValues<SystemPermission>()
            .Where(p => p != SystemPermission.None)
            .Select(p => new PermissionResponse(p.ToString(), (int)p)));
}
