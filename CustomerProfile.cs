using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Functions.Models;

// Azure Table Storage entity for customer profiles.
// PartitionKey groups all customer rows together; RowKey is the unique customer id.
public class CustomerProfile : ITableEntity
{
    public string PartitionKey { get; set; } = "Customer";
    public string RowKey { get; set; } = Guid.NewGuid().ToString();

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}
