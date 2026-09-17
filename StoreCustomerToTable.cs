using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using ABCRetail.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions.Functions;

// Rubric item: "Create a function that stores information in Azure tables"
//
// Reuses the exact same CustomerProfile entity and AzureStorage:* config
// keys as Project 1's TableStorageService, so it writes into the same
// "AbcRetailRecords" table your web app already reads from — add a customer
// here and it will show up in the web app's Customers page immediately.
//
// Sample request (POST):
// {
//   "fullName": "Thabo Mokoena",
//   "email": "thabo@example.com",
//   "phone": "0821234567",
//   "shippingAddress": "12 Main Rd, Johannesburg"
// }

public class StoreCustomerToTable
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public StoreCustomerToTable(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<StoreCustomerToTable>();
        _configuration = configuration;
    }

    [Function("StoreCustomerToTable")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("StoreCustomerToTable function triggered.");

        string body = await new StreamReader(req.Body).ReadToEndAsync();
        var customer = JsonSerializer.Deserialize<CustomerProfile>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (customer is null || string.IsNullOrWhiteSpace(customer.FullName))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("fullName is required.");
            return badRequest;
        }

        // Ensure PartitionKey/RowKey are set even if the caller didn't supply them
        customer.PartitionKey = "Customer";
        if (string.IsNullOrWhiteSpace(customer.RowKey))
        {
            customer.RowKey = Guid.NewGuid().ToString();
        }

        var connectionString = _configuration["AzureStorage:ConnectionString"];
        var tableName = _configuration["AzureStorage:TableName"] ?? "AbcRetailRecords";

        var serviceClient = new TableServiceClient(connectionString);
        await serviceClient.CreateTableIfNotExistsAsync(tableName);
        var tableClient = serviceClient.GetTableClient(tableName);

        await tableClient.AddEntityAsync(customer);

        _logger.LogInformation("Stored customer {Name} with RowKey {RowKey}",
            customer.FullName, customer.RowKey);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { message = "Customer stored.", customer.RowKey });
        return response;
    }
}
