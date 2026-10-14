using InvoiceFlow.BuildingBlocks.Authorization;
using System.Text.Json.Serialization;

namespace InvoiceFlow.Identity.Api;

public record CreateRoleRequest(
    [property: JsonRequired] string Name, 
    string? Description, 
    SystemPermissions Permissions = SystemPermissions.None);
public record UpdateRoleRequest(
    [property: JsonRequired] string Name, 
    string? Description);
public record UpdatePermissionsRequest(
    [property: JsonRequired] SystemPermissions Permissions);