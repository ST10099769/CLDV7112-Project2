using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions.Functions;

// Rubric item: "Create a function that writes to Azure blob storage"
//
// Writes into the same "product-media" container Project 1's
// BlobStorageService/MediaController already uses — an image uploaded here
// will show up in the web app's Media gallery immediately.
//
// Sample request (POST):
// Raw body = the image bytes
// Query string: ?fileName=sneaker.jpg&contentType=image/jpeg

public class UploadProductImage
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public UploadProductImage(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<UploadProductImage>();
        _configuration = configuration;
    }

    [Function("UploadProductImage")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("UploadProductImage function triggered.");

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string originalFileName = query["fileName"] ?? "upload.bin";
        string contentType = query["contentType"] ?? "application/octet-stream";

        // Same naming convention as BlobStorageService.UploadImageAsync
        string blobName = $"{Guid.NewGuid()}-{originalFileName}";

        var connectionString = _configuration["AzureStorage:ConnectionString"];
        var containerName = _configuration["AzureStorage:BlobContainerName"] ?? "product-media";

        var serviceClient = new BlobServiceClient(connectionString);
        var containerClient = serviceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(req.Body, new BlobHttpHeaders { ContentType = contentType });

        _logger.LogInformation("Uploaded blob {BlobName} to container {Container}",
            blobName, containerName);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            message = "File uploaded to blob storage.",
            blobName,
            blobUrl = blobClient.Uri.ToString()
        });
        return response;
    }
}
