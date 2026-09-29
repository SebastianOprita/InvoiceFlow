using InvoiceFlow.Customers.Domain;

namespace InvoiceFlow.Customers.Infrastructure;

public static class CustomersDbSeed
{
    public static async Task SeedAsync(CustomersDbContext db)
    {
        var tenantId = Guid.Parse("f305e3f0-e634-470b-b9c1-cc0c7559dc0a");
        var customer1 = Customer.Create(
            Guid.Parse("01a0cae9-2e61-7b82-924b-5beaf5b08d4a"),
            tenantId,
            CustomerCode.Create("CUST001"),
            CustomerName.Create("Customer One"),
            CustomerEmail.Create("one@email.com"),
            PhoneNumber.CreateOptional("+1234567890"),
            TaxNumber.CreateOptional("123456789"),
            RegistrationNumber.Create("REG123456"),
            AddressLine1.Create("123 Main St"),
            AddressLine2.CreateOptional("Suite 100"),
            City.Create("Cityville"),
            State.CreateOptional("Stateville"),
            Country.Create("Countryland"),
            PostalCode.Create("12345"),
            CurrencyCode.Create("USD"),
            CreditLimit.Create(10000m),
            PaymentTermDays.Create(30),
            DateTime.UtcNow
            );

        var customer2 = Customer.Create(
            Guid.Parse("01a0cae9-2e6a-7ef2-a48c-88c69fd80eb8"),
            tenantId,
            CustomerCode.Create("CUST002"),
            CustomerName.Create("Customer Two"),
            CustomerEmail.Create("two@email.com"),
            PhoneNumber.CreateOptional("+1234567890"),
            TaxNumber.CreateOptional("123456788"),
            RegistrationNumber.Create("REG123457"),
            AddressLine1.Create("123 Main St"),
            AddressLine2.CreateOptional("Suite 100"),
            City.Create("Cityville"),
            State.CreateOptional("Stateville"),
            Country.Create("Countryland"),
            PostalCode.Create("12345"),
            CurrencyCode.Create("USD"),
            CreditLimit.Create(10000m),
            PaymentTermDays.Create(30),
            DateTime.UtcNow
            );

        await db.Customers.AddRangeAsync(customer1, customer2);
        await db.SaveChangesAsync();
    }
}
