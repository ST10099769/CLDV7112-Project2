using System.Text;
using System.Text.Json;
using ABCRetail.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions.Functions;

// Rubric item: "Create a function that reads from/writes to the Azure queue"
// This is the READ half — fires automatically whenever a new message lands
// in "order-processing-queue" (i.e. whenever SendOrderMessage.cs runs, or
// whenever the Order Queue page in Project 1's web app sends a message).
//
// For your screenshot: trigger SendOrderMessage (or use the web app's Order
// Queue form) first, then watch this function's logs — func start console,
// or Portal > Function App > Monitor — to show the message being read.
//
// NOTE: the "Connection" value here must be a FLAT app setting containing
// the connection string (not the nested AzureStorage:ConnectionString used
// elsewhere) — that's what "AzureStorageConnection" in local.settings.json
// is for.

public class ProcessOrderMessage
{
    private readonly ILogger _logger;

    public ProcessOrderMessage(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<ProcessOrderMessage>();
    }

    [Function("ProcessOrderMessage")]
    public void Run(
        [QueueTrigger("order-processing-queue", Connection = "AzureStorageConnection")] string queueMessage)
    {
        _logger.LogInformation("ProcessOrderMessage triggered with raw message: {Message}", queueMessage);

        try
        {
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(queueMessage));
            var order = JsonSerializer.Deserialize<OrderQueueMessage>(decoded,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (order is not null)
            {
                _logger.LogInformation(
                    "Processed order {OrderId} ({MessageType}) for product {ProductName} x{Quantity}, status {Status}",
                    order.OrderId, order.MessageType, order.ProductName, order.Quantity, order.Status);
            }
        }
        catch (FormatException)
        {
            _logger.LogInformation("Processed raw (non-base64) message: {Message}", queueMessage);
        }
    }
}
