using MassTransit;
using RoutingSlipSample.Api.Components.Activities.GrillBurger;
using RoutingSlipSample.Contracts;

namespace RoutingSlipSample.Api.Components.Activities.DressBurger
{
    public class DressBurgerActivity : IActivity<IDressBurgerArguments, DressBurgerLog>
    {
        private readonly ILogger<DressBurgerActivity> _logger;

        public DressBurgerActivity(ILogger<DressBurgerActivity> logger)
        {
            _logger = logger;
        }
        public async Task<ExecutionResult> Execute(ExecuteContext<IDressBurgerArguments> context)
        {
            _logger.LogDebug($"Dress Burger: {context.Arguments.OrderId},");
            await Task.Delay(1000);
            return context.Completed();
           // return context.Faulted();
        }

        public Task<CompensationResult> Compensate(CompensateContext<DressBurgerLog> context)
        {
            throw new NotImplementedException();
        }
    }

    public interface IDressBurgerArguments
    {
        Guid OrderId { get; }
        BurgerPatty Patty { get; }
        bool Pickles { get; }
        bool Lettuce { get; }
        bool Cheese { get; }
    }

    public class DressBurgerLog
    {
    }
}
