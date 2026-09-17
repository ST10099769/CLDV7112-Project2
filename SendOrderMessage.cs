using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using ABCRetail.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions.Functions;

// Rubric item: "Create a function that reads from/writes to the Azure queue"
// This is the WRITE half. See ProcessOrderMessage.cs for the READ half.
//
// Uses the same OrderQueueMessage shape and "order-processing-queue" queue
// as Project 1's QueueStorageService/OrderQueueController — a message sent
// here will appear in the web app's Order Queue page immediately.
//
// Sample request (POST):
// {
//   "messageType": "ProcessingOrder",
//   "orderId": "ORD-1001",
//   "productName": "Running Shoes",
//   "quantity": 2,
//   "status": "Pending"
// }

public class SendOrderMessage
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public SendOrderMessage(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<SendOrderMessage>();
        _configuration = configuration;
    }

    [Function("SendOrderMessage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("SendOrderMessage function triggered.");

        string body = await new StreamReader(req.Body).ReadToEndAsync();
        var message = JsonSerializer.Deserialize<OrderQueueMessage>(body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (message is null || string.IsNullOrWhiteSpace(message.OrderId))
        {
            var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequest.WriteStringAsync("orderId is required.");
            return badRequest;
        }

        message.CreatedAtUtc = DateTime.UtcNow;

        var connectionString = _configuration["AzureStorage:ConnectionString"];
        var queueName = _configuration["AzureStorage:QueueName"] ?? "order-processing-queue";

        var queueClient = new QueueClient(connectionString, queueName);
        await queueClient.CreateIfNotExistsAsync();

        string messageJson = JsonSerializer.Serialize(message);
        await queueClient.SendMessageAsync(
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageJson)));

        _logger.LogInformation("Sent order {OrderId} to queue {QueueName}",
            message.OrderId, queueName);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new { message = "Order queued.", orderId = message.OrderId });
        return response;
    }
}
