namespace Contracts;

public record SubmitOrder
{
    public Guid OrderId { get; init; }
    public DateTimeOffset Timestamp { get; init; }
    public string OrderNumber { get; init; }
}

