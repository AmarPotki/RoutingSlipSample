using MassTransit;

namespace RegistrationRoutingSlipSample.Api.Components;

public interface IEndpointAddressProvider
{
    Uri GetExecuteEndpoint<T, TArguments>()
        where T : class, IExecuteActivity<TArguments>
        where TArguments : class;
}