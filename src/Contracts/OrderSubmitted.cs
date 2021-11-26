namespace Contracts;
public record OrderSubmitted
{
    public Guid OrderId { get; init; }
    public DateTimeOffset OrderTimestamp { get; init; }

    public string OrderNumber { get; init; }
}
