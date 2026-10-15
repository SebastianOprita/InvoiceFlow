using InvoiceFlow.BuildingBlocks.Domain;

namespace InvoiceFlow.Invoices.Domain;

public static class UnitOfMeasureExtensions
{
    public static string GetSymbol(this UnitOfMeasure unit) =>
        unit switch
        {
            UnitOfMeasure.Piece => "pcs",
            UnitOfMeasure.Hour => "h",
            UnitOfMeasure.Day => "day",
            UnitOfMeasure.Month => "month",
            UnitOfMeasure.Kilogram => "kg",
            UnitOfMeasure.Gram => "g",
            UnitOfMeasure.Meter => "m",
            UnitOfMeasure.SquareMeter => "m²",
            UnitOfMeasure.CubicMeter => "m³",
            UnitOfMeasure.Liter => "L",
            UnitOfMeasure.Service => "service",
            UnitOfMeasure.License => "license",
            _ => throw new DomainException(DomainErrors.UnitOfMeasureInvalid)
        };
}
