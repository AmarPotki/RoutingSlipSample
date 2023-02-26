using MassTransit;

namespace RoutingSlipSample.Contracts;

[ExcludeFromTopology]
public interface OrderLine
{
    Guid OrderId { get; }
    Guid OrderLineId { get; }
}