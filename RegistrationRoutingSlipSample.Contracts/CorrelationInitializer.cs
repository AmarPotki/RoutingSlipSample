using MassTransit;
using System.Runtime.CompilerServices;

namespace RegistrationRoutingSlipSample.Contracts;


public static class CorrelationInitializer
{
    //[ModuleInitializer]
    //public static void Initialize()
    //{
    //    MessageCorrelation.UseCorrelationId<GetRegistrationStatus>(x => x.SubmissionId);
    //    MessageCorrelation.UseCorrelationId<ProcessRegistration>(x => x.SubmissionId);
    //    MessageCorrelation.UseCorrelationId<RegistrationStatus>(x => x.SubmissionId);
    //    MessageCorrelation.UseCorrelationId<SubmitRegistration>(x => x.SubmissionId);

    //}
}

