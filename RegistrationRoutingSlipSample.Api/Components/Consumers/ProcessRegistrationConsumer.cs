using MassTransit;
using MassTransit.Courier.Contracts;
using RegistrationRoutingSlipSample.Api.Components.Activities;
using RegistrationRoutingSlipSample.Contracts;

namespace RegistrationRoutingSlipSample.Api.Components.Consumers;

public class ProcessRegistrationConsumer : IConsumer<ProcessRegistration>
{
    readonly ILogger<ProcessRegistrationConsumer> _logger;
    readonly IEndpointAddressProvider _provider;
    readonly ISecurePaymentInfoService _paymentInfoService;

    public ProcessRegistrationConsumer(ILogger<ProcessRegistrationConsumer> logger, IEndpointAddressProvider provider, ISecurePaymentInfoService paymentInfoService)
    {
        _logger = logger;
        _provider = provider;
        _paymentInfoService = paymentInfoService;
    }

    public async Task Consume(ConsumeContext<ProcessRegistration> context)
    {
        _logger.LogInformation("Processing registration: {0} ({1})", context.Message.SubmissionId, context.Message.ParticipantEmailAddress);
        var routingSlip = CreateRoutingSlip(context);

        await context.Execute(routingSlip).ConfigureAwait(false);
    }

    private RoutingSlip CreateRoutingSlip(ConsumeContext<ProcessRegistration> context)
    {
        var builder = new RoutingSlipBuilder(NewId.NextGuid());
        builder.SetVariables(new
        {
            context.Message.ParticipantEmailAddress,
            context.Message.ParticipantLicenseNumber,
            context.Message.ParticipantCategory,
        });

        if (!string.IsNullOrWhiteSpace(context.Message.ParticipantLicenseNumber))
        {
            builder.AddActivity("LicenseVerification", _provider.GetExecuteEndpoint<LicenseVerificationActivity, LicenseVerificationArguments>(),
                new { EventType = "Road" });
            builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.ActivityFaulted, RoutingSlipEventContents.None,
                // update saga
                x => x.Send<RegistrationLicenseVerificationFailed>(new { context.Message.SubmissionId }));
        }
        builder.AddActivity("EventRegistration", _provider.GetExecuteEndpoint<EventRegistrationActivity, EventRegistrationArguments>(),
            new
            {
                context.Message.EventId,
                context.Message.RaceId
            });
        var paymentInfo = _paymentInfoService.GetPaymentInfo(context.Message.ParticipantEmailAddress, context.Message.CardNumber);

        builder.AddActivity("ProcessPayment", _provider.GetExecuteEndpoint<ProcessPaymentActivity, ProcessPaymentArguments>(), paymentInfo);

        builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.ActivityFaulted, RoutingSlipEventContents.None, "ProcessPayment",
            x => x.Send<RegistrationPaymentFailed>(new { context.Message.SubmissionId }));


        builder.AddSubscription(context.SourceAddress, RoutingSlipEvents.Completed,
            x => x.Send<RegistrationCompleted>(new { context.Message.SubmissionId }));
        return builder.Build();
    }
}

public interface ISecurePaymentInfoService
{
    SecurePaymentInfo GetPaymentInfo(string messageParticipantEmailAddress, string messageCardNumber);
}
public class SecurePaymentInfoService :
    ISecurePaymentInfoService
{
    public SecurePaymentInfo GetPaymentInfo(string emailAddress, string cardNumber)
    {
        return new SecurePaymentInfo
        {
            CardNumber = cardNumber,
            VerificationCode = "123",
            CardholderName = "FRANK UNDERHILL",
            ExpirationMonth = 12,
            ExpirationYear = 2023,
        };
    }
}

public class SecurePaymentInfo
{
    public string CardNumber { get; set; }
    public string VerificationCode { get; set; }
    public string CardholderName { get; set; }
    public int ExpirationMonth { get; set; }
    public int ExpirationYear { get; set; }
}