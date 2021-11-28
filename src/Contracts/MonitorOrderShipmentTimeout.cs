namespace Contracts;
public record MonitorOrderShipmentTimeout
{
    public Guid OrderId { get; init; }
}