namespace InvoiceFlow.Identity.Api;

public record CreatePlatformUserRequest(string Email, string Password, string FirstName, string LastName);
public record UpdatePlatformUserRequest(string FirstName, string LastName);
