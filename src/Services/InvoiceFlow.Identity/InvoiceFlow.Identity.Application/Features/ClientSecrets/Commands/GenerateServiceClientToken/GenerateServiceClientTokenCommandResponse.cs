namespace InvoiceFlow.Identity.Application;

public sealed record GenerateServiceClientTokenCommandResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset AccessTokenExpiresAt);
