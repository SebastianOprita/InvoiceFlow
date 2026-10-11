namespace InvoiceFlow.Identity.Api;

public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);