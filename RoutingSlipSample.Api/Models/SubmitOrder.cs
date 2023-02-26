using MassTransit;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace RoutingSlipSample.Api.Models;


public class OrderModel
{
    [Required]
    public Guid OrderId { get; init; }
}
public class SubmitOrderResponseModel
{
    public Guid OrderId { get; init; }
}
public interface OrderSubmissionAccepted
{
    Guid OrderId { get; }

    [ModuleInitializer]
    internal static void Init()
    {
        GlobalTopology.Send.UseCorrelationId<OrderSubmissionAccepted>(x => x.OrderId);
    }
}