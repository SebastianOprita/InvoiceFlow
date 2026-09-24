namespace InvoiceFlow.BuildingBlocks.Application;

public sealed record ErrorDefinitionResponse(
    string Code,
    string Message,
    string Source,
    int SuggestedStatus);
