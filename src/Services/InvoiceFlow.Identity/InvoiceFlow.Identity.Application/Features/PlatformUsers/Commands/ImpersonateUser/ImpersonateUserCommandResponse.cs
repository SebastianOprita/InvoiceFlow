namespace InvoiceFlow.Identity.Application;

public record ImpersonateUserCommandResponse(
    string ImpersonationToken,
    DateTime ImpersonationTokenExpiresAt);
