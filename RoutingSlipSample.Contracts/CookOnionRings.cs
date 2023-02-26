namespace RoutingSlipSample.Contracts;

public interface CookOnionRings
{
    Guid OrderId { get; }
    Guid OrderLineId { get; }

    int Quantity { get; }
}