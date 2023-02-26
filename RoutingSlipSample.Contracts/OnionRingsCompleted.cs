namespace RoutingSlipSample.Contracts;

public interface OnionRingsCompleted :
    OrderLineCompleted
{
    int Quantity { get; }
}