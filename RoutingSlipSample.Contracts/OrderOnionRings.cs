namespace RoutingSlipSample.Contracts;

public interface OrderOnionRings :
    OrderLine
{
    int Quantity { get; }
}