namespace RegistrationRoutingSlipSample.Api.Components.Activities;

public class LicenseVerificationArguments
{
    public string ParticipantLicenseNumber { get; init; }

    public string EventType { get; init; }
    public string ParticipantCategory { get; init; }
}