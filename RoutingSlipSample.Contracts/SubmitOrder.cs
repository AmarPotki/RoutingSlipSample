namespace RoutingSlipSample.Contracts;

public interface SubmitOrder
{
    Guid OrderId { get; }

    //Burger[] Burgers { get; }
    //Fry[] Fries { get; }
    //Shake[] Shakes { get; }
    //FryShake[] FryShakes { get; }
}

public interface SubmitOrderV2
{
    Guid OrderId { get; }


}