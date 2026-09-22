namespace InvoiceFlow.Common.Domain;

public sealed record DomainError(string ErrorCode, string ErrorMessage);