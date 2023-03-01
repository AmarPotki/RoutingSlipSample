﻿using MassTransit;

namespace RegistrationRoutingSlipSample.Api.Components.Activities;

public class EventRegistrationActivity : IActivity<EventRegistrationArguments, EventRegistrationLog>
{
    private readonly ILogger<EventRegistrationActivity> _logger;
    private readonly IEndpointAddressProvider _provider;

    public EventRegistrationActivity(ILogger<EventRegistrationActivity> logger, IEndpointAddressProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }
    public async Task<ExecutionResult> Execute(ExecuteContext<EventRegistrationArguments> context)
    {
        var arguments = context.Arguments;

        _logger.LogInformation("Registering for event: {EventId} ({Email})", arguments.EventId, arguments.ParticipantEmailAddress);

        var registrationTotal = 25.00m;

        if (!string.IsNullOrWhiteSpace(arguments.ParticipantLicenseNumber))
        {
            _logger.LogInformation("Participant Detail: {LicenseNumber} ({LicenseExpiration}) {Category}",
                arguments.ParticipantLicenseNumber, arguments.ParticipantLicenseExpirationDate, arguments.ParticipantCategory);

            registrationTotal = 15.0m;
        }

        await Task.Delay(10);

        Guid? registrationId = NewId.NextGuid();

        _logger.LogInformation("Registered for event: {RegistrationId} ({Email})", registrationId, arguments.ParticipantEmailAddress);

        var log = new EventRegistrationLog
        {
            RegistrationId = registrationId.Value,
            ParticipantEmailAddress = arguments.ParticipantEmailAddress
        };

        var variables = new
        {
            registrationId,
            Amount = registrationTotal
        };

        if (arguments.EventId?.StartsWith("DANGER") ?? false)
        {
            //return context.ReviseItinerary(log, variables, x =>
            //{
            //    x.AddActivitiesFromSourceItinerary();
            //    x.AddActivity("Assign Waiver", _provider.GetExecuteEndpoint<AssignWaiverActivity, AssignWaiverArguments>());
            //});
        }

        return context.CompletedWithVariables(new
        {
            registrationId,
            arguments.ParticipantEmailAddress
        }, variables);
    }

    public async Task<CompensationResult> Compensate(CompensateContext<EventRegistrationLog> context)
    {
        _logger.LogInformation("Removing registration for event: {RegistrationId} ({Email})", context.Log.RegistrationId, context.Log.ParticipantEmailAddress);

        await Task.Delay(10);

        return context.Compensated();
    }
}

public class EventRegistrationLog
{
    public Guid RegistrationId { get; init; }
    public string ParticipantEmailAddress { get; init; }
}

public class EventRegistrationArguments
{
    public string ParticipantEmailAddress { get; init; }

    public string ParticipantLicenseNumber { get; init; }
    public DateTime? ParticipantLicenseExpirationDate { get; init; }

    public string ParticipantCategory { get; init; }

    public string EventId { get; init; }
    public string RaceId { get; init; }
}