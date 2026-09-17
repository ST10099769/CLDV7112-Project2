namespace ABCRetail.Functions.Models;

// Shape of the JSON message placed on the Azure Storage Queue.
// Used both for order processing and inventory management events,
// distinguished by MessageType.
public class OrderQueueMessage
{
    public string MessageType { get; set; } = "ProcessingOrder"; // e.g. "ProcessingOrder" or "InventoryUpdate"
    public string OrderId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
