using MassTransit;

namespace RegistrationRoutingSlipSample.Contracts;

public class RegistrationLicenseVerificationFailed
{
    public Guid SubmissionId { get; init; }

    public ExceptionInfo ExceptionInfo { get; init; }
}