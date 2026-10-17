namespace InvoiceFlow.Payments.Domain;

public enum PaymentMethod
{
    BankTransfer = 1,
    Cash = 2,
    Card = 3,
    DirectDebit = 4,
    Cheque = 5,
    Other = 6
}
