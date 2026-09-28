namespace InvoiceFlow.BuildingBlocks.Api;

public sealed record ErrorDefinitionResponse(
    string Code,
    string Message,
    string Source,
    int SuggestedStatus);
