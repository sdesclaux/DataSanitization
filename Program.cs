using DataSanitization;
using Microsoft.Extensions.Compliance.Classification;
using Microsoft.Extensions.Compliance.Redaction;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options => options.JsonWriterOptions = new JsonWriterOptions
{
    Indented = true,
});

builder.Logging.EnableRedaction();
builder.Services.AddRedaction(x =>
{
    //x.SetRedactor<ErasingRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData));
    x.SetRedactor<StarRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveData));

    x.SetHmacRedactor(options =>
    {
        options.Key = Convert.ToBase64String("SecretToStoreInAVaultAndBeAtLeastOfALengthOf44"u8);
        options.KeyId = 69;
    }, new DataClassificationSet(DataTaxonomy.PiiData));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGet("/customer", () => "pong");

app.MapPost("/customers", (Customer customer, ILogger<Program> logger) =>
{
    //Customer created here

    //logger.LogInformation("Customer created {Customer}", customer);
    logger.LogCustomerCreated(customer);
    return customer;
});

app.Run();

public record Customer(
    [SensitiveData] string Name,
    [PiiData] string Email,
    DateOnly DateOfBirth)
{
    public Guid Id { get; } = Guid.NewGuid();
}
