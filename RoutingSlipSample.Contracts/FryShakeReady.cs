namespace RoutingSlipSample.Contracts;

public interface FryShakeReady
{
    Guid OrderId { get; }
    Guid OrderLineId { get; }
    string Flavor { get; }
    Size Size { get; }
}