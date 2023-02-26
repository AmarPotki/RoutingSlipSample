namespace RoutingSlipSample.Contracts;

public interface OrderShake :
    OrderLine
{
    string Flavor { get; }
    Size Size { get; }
}