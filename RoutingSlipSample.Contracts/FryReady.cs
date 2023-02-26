namespace RoutingSlipSample.Contracts;

public interface FryReady
{
    Guid OrderId { get; }
    Guid OrderLineId { get; }
    Size Size { get; }
}