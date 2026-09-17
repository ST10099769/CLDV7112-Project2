using System.Net;
using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABCRetail.Functions.Functions;

// Rubric item: "Create a function that sends a file to Azure Files"
//
// Writes into the same "abcretaillogs" share / "logs" directory as
// Project 1's FileStorageService/LogsController — a file written here will
// show up in the web app's Logs page immediately.
//
// Sample request (POST):
// Raw body = the log/file content (text or binary)
// Query string: ?fileName=order-ORD-1001-log.txt   (optional — defaults to a timestamped name)

public class UploadOrderFile
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public UploadOrderFile(ILoggerFactory loggerFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<UploadOrderFile>();
        _configuration = configuration;
    }

    [Function("UploadOrderFile")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req)
    {
        _logger.LogInformation("UploadOrderFile function triggered.");

        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        string fileName = query["fileName"] ?? $"log-{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}.txt";

        var connectionString = _configuration["AzureStorage:ConnectionString"];
        var shareName = _configuration["AzureStorage:FileShareName"] ?? "abcretaillogs";
        var directoryName = _configuration["AzureStorage:FileShareDirectory"] ?? "logs";

        var shareClient = new ShareClient(connectionString, shareName);
        await shareClient.CreateIfNotExistsAsync();

        var directoryClient = shareClient.GetRootDirectoryClient().GetSubdirectoryClient(directoryName);
        await directoryClient.CreateIfNotExistsAsync();

        var fileClient = directoryClient.GetFileClient(fileName);

        // Buffer the body so we know its length (Azure Files requires a fixed size on create)
        using var memoryStream = new MemoryStream();
        await req.Body.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        await fileClient.CreateAsync(memoryStream.Length);
        await fileClient.UploadRangeAsync(new Azure.HttpRange(0, memoryStream.Length), memoryStream);

        _logger.LogInformation("Uploaded file {FileName} to share {ShareName}/{Directory}",
            fileName, shareName, directoryName);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            message = "File uploaded to Azure Files.",
            fileName,
            fileUrl = fileClient.Uri.ToString()
        });
        return response;
    }
}
