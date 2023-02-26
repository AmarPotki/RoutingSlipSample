namespace RoutingSlipSample.Contracts;

public interface BurgerCompleted :
    OrderLineCompleted
{
    Burger Burger { get; }
}