using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Functions.Models;

// Azure Table Storage entity for product-related information.
public class Product : ITableEntity
{
    public string PartitionKey { get; set; } = "Product";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }

    // Name of the blob (in Blob Storage) that holds this product's image, if any.
    public string? ImageBlobName { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
