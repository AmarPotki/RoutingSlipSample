using MassTransit;
using MassTransit.Courier.Contracts;
using MassTransit.Events;
using MassTransit.Metadata;
using RoutingSlipSample.Api.Components.Activities.DressBurger;
using RoutingSlipSample.Api.Components.Activities.GrillBurger;
using RoutingSlipSample.Api.Models;
using RoutingSlipSample.Contracts;

namespace RoutingSlipSample.Api.Components.Consumers;

public class SubmitOrderWaitUntilRoutingSlipFinishConsumer :
    IConsumer<SubmitOrderV2>,
    IConsumer<RoutingSlipCompleted>,
    IConsumer<RoutingSlipFaulted>
{
    private readonly ILogger<SubmitOrderWaitUntilRoutingSlipFinishConsumer> _logger;
    private readonly IEndpointNameFormatter _formatter;

    public SubmitOrderWaitUntilRoutingSlipFinishConsumer(ILogger<SubmitOrderWaitUntilRoutingSlipFinishConsumer> logger, IEndpointNameFormatter formatter)
    {
        _logger = logger;
        _formatter = formatter;
    }
    public async Task Consume(ConsumeContext<SubmitOrderV2> context)
    {
        _logger.LogDebug("Order Submission Received: {OrderId} {CorrelationId}", context.Message.OrderId, context.CorrelationId);

        var routingSlip = CreateRoutingSlip(context);
        await context.Execute(routingSlip);
    }


    RoutingSlip CreateRoutingSlip(ConsumeContext<SubmitOrderV2> context)
    {
        var builder = new RoutingSlipBuilder(Guid.NewGuid());
        builder.AddSubscription(context.ReceiveContext.InputAddress, RoutingSlipEvents.ActivityCompleted | RoutingSlipEvents.ActivityFaulted);


        builder.AddVariable("OrderId", context.Message.OrderId);
        builder.AddVariable("Request", context.Message);
        builder.AddVariable(nameof(ConsumeContext.RequestId), context.RequestId);
        builder.AddVariable(nameof(ConsumeContext.ResponseAddress), context.ResponseAddress);
        builder.AddVariable(nameof(ConsumeContext.FaultAddress), context.FaultAddress);


        var grillQueueName = _formatter.ExecuteActivity<GrillBurgerActivity, IGrillBurgerArguments>();
        builder.AddActivity("grill-burger", new Uri($"queue:{grillQueueName}"), new
        {
            Weight = 0.5m,
            WeightTemperature = 165
        });
        var dressQueueName = _formatter.ExecuteActivity<DressBurgerActivity, IDressBurgerArguments>();
        builder.AddActivity("dress-burger", new Uri($"queue:{dressQueueName}"), new
        {
            Pickles = true,
            Lettuce = true
        });
        return builder.Build();
    }

    public async Task Consume(ConsumeContext<RoutingSlipCompleted> context)
    {

        var requestId = context.GetVariable<Guid>(nameof(ConsumeContext.RequestId));
        var orderId = context.GetVariable<Guid>("OrderId");

        var responseAddress = context.GetVariable<Uri>(nameof(ConsumeContext.ResponseAddress));
        if (requestId.HasValue && responseAddress != null)
        {


            var responseEndpoint = await context.GetResponseEndpoint<OrderSubmissionAcceptedV2>(responseAddress);

            await responseEndpoint.Send<OrderSubmissionAcceptedV2>(new { OrderId = orderId });
        }
    }

    public async Task Consume(ConsumeContext<RoutingSlipFaulted> context)
    {
        var requestId = context.GetVariable<Guid>(nameof(ConsumeContext.RequestId));
        var responseAddress = context.GetVariable<Uri>(nameof(ConsumeContext.ResponseAddress));
        var request = context.GetVariable<SubmitOrderV2>("Request");

        if (requestId.HasValue && responseAddress != null)
        {
            var responseEndpoint = await context.GetResponseEndpoint<OrderSubmissionAcceptedV2>(responseAddress);

            IEnumerable<ExceptionInfo> exceptions = context.Message.ActivityExceptions.Select(x => x.ExceptionInfo);

            Fault<SubmitOrderV2> response =
                new FaultEvent<SubmitOrderV2>(request, requestId, context.Host, exceptions,
                    TypeMetadataCache<SubmitOrderV2>.MessageTypeNames);

            await responseEndpoint.Send(response);
        }
    }
}