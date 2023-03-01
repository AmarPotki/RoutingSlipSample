using MassTransit;

namespace RegistrationRoutingSlipSample.Api.Components.StateMachines;

public class RegistrationState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    #region RegistrationSubmitProperties

    public string ParticipantEmailAddress { get; set; }
    public string ParticipantLicenseNumber { get; set; }
    public string ParticipantCategory { get; set; }
    public string CardNumber { get; set; } = "";
    public string EventId { get; set; }
    public string RaceId { get; set; }
    public Guid? RegistrationId { get; set; }

    #endregion


    public DateTime? ParticipantLicenseExpirationDate { get; set; }
    public string Reason { get; set; } = "";
    public int RetryAttempt { get; set; } = 0;
    public Guid? ScheduleRetryToken { get; set; }

}