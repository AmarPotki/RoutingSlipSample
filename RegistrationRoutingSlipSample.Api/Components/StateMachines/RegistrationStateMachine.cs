﻿using MassTransit;
using RegistrationRoutingSlipSample.Contracts;

namespace RegistrationRoutingSlipSample.Api.Components.StateMachines;

public class RegistrationStateMachine : MassTransitStateMachine<RegistrationState>
{
    public RegistrationStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => EventRegistrationReceived, x
            => x.CorrelateById(context => context.Message.SubmissionId));
        Event(() => RegistrationStatusRequested, x
            => x.CorrelateById(context => context.Message.SubmissionId));
        Event(() => LicenseVerificationFailed, x
            => x.CorrelateById(context => context.Message.SubmissionId));
        Event(() => EventRegistrationCompleted, x
            => x.CorrelateById(context => context.Message.SubmissionId));
        Event(() => PaymentFailed, x
            => x.CorrelateById(context => context.Message.SubmissionId));

        Initially(
            When(EventRegistrationReceived)
                .Initialize()
                .InitiateProcessing()
                .TransitionTo(Received));

        During(Received,
            When(LicenseVerificationFailed)
                .InvalidLicense()
                .TransitionTo(Suspended),
            When(EventRegistrationCompleted)
                .Registered()
                .TransitionTo(Registered),
            When(PaymentFailed)
                .PaymentFailed()
                .TransitionTo(Suspended));

        DuringAny(When(RegistrationStatusRequested).Respond(x => new RegistrationStatus
        {
            SubmissionId = x.Saga.CorrelationId,
            ParticipantEmailAddress = x.Saga.ParticipantEmailAddress,
            ParticipantLicenseNumber = x.Saga.ParticipantLicenseNumber,
            ParticipantLicenseExpirationDate = x.Saga.ParticipantLicenseExpirationDate,
            RegistrationId = x.Saga.RegistrationId,
            EventId = x.Saga.EventId,
            RaceId = x.Saga.RaceId,
            Status = x.Saga.CurrentState
        })

        );

        Schedule(()=>RetryDelayExpired,saga=>saga.ScheduleRetryToken, x =>
        {
            x.Received = r =>
            {
                r.CorrelateById(context => context.Message.ExpirationId);
                r.ConfigureConsumeTopology = false;
            };
        });

    }


    public State Received { get; }
    public State Registered { get; }
    public State WaitingToRetry { get; }
    public State Suspended { get; }


    public Event<RegistrationReceived> EventRegistrationReceived { get; }
    public Event<GetRegistrationStatus> RegistrationStatusRequested { get; }
    public Event<RegistrationLicenseVerificationFailed> LicenseVerificationFailed { get; }
    public Event<RegistrationCompleted> EventRegistrationCompleted { get; }
    public Event<RegistrationPaymentFailed> PaymentFailed { get; }
    public Schedule<RegistrationState, RetryDelayExpired> RetryDelayExpired { get; set; }
}

static class RegistrationStateMachineBehaviorExtensions
{

    public static EventActivityBinder<RegistrationState, RegistrationReceived> Initialize(
        this EventActivityBinder<RegistrationState, RegistrationReceived> binder)
    {
        return binder.Then(context =>
        {
            context.Saga.ParticipantEmailAddress = context.Message.ParticipantEmailAddress;
            context.Saga.ParticipantLicenseNumber = context.Message.ParticipantLicenseNumber;
            context.Saga.ParticipantCategory = context.Message.ParticipantCategory;

            context.Saga.EventId = context.Message.EventId;
            context.Saga.RaceId = context.Message.RaceId;
            context.Saga.CardNumber = context.Message.CardNumber;

            LogContext.Info?.Log("Processing: {0} ({1})", context.Message.SubmissionId, context.Message.ParticipantEmailAddress);
        });
    }
    public static EventActivityBinder<RegistrationState, RegistrationReceived> InitiateProcessing(
        this EventActivityBinder<RegistrationState, RegistrationReceived> binder)
    {
        return binder.PublishAsync(context => context.Init<ProcessRegistration>(context.Message));
    }
    public static EventActivityBinder<RegistrationState, RegistrationLicenseVerificationFailed> InvalidLicense(
        this EventActivityBinder<RegistrationState, RegistrationLicenseVerificationFailed> binder)
    {
        return binder.Then(context =>
        {
            LogContext.Info?.Log("Invalid License: {0} ({1}) - {2}", context.Message.SubmissionId, context.Saga.ParticipantLicenseNumber,
                context.Message.ExceptionInfo.Message);

            context.Saga.Reason = "Invalid License";
        });
    }

    public static EventActivityBinder<RegistrationState, RegistrationCompleted> Registered(
        this EventActivityBinder<RegistrationState, RegistrationCompleted> binder)
    {
        return binder.Then(context =>
        {
            LogContext.Info?.Log("Registered: {0} ({1})", context.Message.SubmissionId, 
                context.Saga.ParticipantEmailAddress);

            context.Saga.ParticipantLicenseExpirationDate = context.GetVariable<DateTime>("ParticipantLicenseExpirationDate");
            context.Saga.RegistrationId = context.GetVariable<Guid>("RegistrationId");
        });
    }
    public static EventActivityBinder<RegistrationState, RegistrationPaymentFailed> PaymentFailed(
        this EventActivityBinder<RegistrationState, RegistrationPaymentFailed> binder)
    {
        return binder.Then(context =>
        {
            LogContext.Info?.Log("Payment Failed: {0} ({1}) - {2}", context.Message.SubmissionId, context.Saga.ParticipantEmailAddress,
                context.Message.ExceptionInfo.Message);

            context.Saga.Reason = "Payment Failed";
        });
    }
}