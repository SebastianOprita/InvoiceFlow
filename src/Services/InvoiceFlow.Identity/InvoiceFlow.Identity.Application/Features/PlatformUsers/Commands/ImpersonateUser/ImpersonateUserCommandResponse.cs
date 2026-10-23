namespace InvoiceFlow.Identity.Application;

public sealed record ImpersonateUserCommandResponse(
    string ImpersonationToken,
    DateTimeOffset ImpersonationTokenExpiresAt);
