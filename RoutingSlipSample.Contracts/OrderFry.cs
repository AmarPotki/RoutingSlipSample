namespace RoutingSlipSample.Contracts;

public interface OrderFry :
    OrderLine
{
    Size Size { get; }
}