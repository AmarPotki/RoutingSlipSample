using MassTransit;

namespace RoutingSlipSample.Contracts;

public interface OrderFaulted :
    FutureFaulted
{
    Guid OrderId { get; }

    IDictionary<Guid, OrderLineCompleted> LinesCompleted { get; }

    IDictionary<Guid, Fault<OrderLine>> LinesFaulted { get; }
}