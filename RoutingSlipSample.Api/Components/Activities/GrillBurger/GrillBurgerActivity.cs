using MassTransit;
using RoutingSlipSample.Contracts;

namespace RoutingSlipSample.Api.Components.Activities.GrillBurger
{
    public class GrillBurgerActivity : IActivity<IGrillBurgerArguments, GrillBurgerLog>
    {
        private readonly ILogger<GrillBurgerActivity> _logger;

        public GrillBurgerActivity(ILogger<GrillBurgerActivity> logger)
        {
            _logger = logger;
        }
        public async Task<ExecutionResult> Execute(ExecuteContext<IGrillBurgerArguments> context)
        {
            _logger.LogDebug($"GrillBurger:{context.Arguments.OrderId}, {context.Arguments.Weight}");
            await Task.Delay(1000);
            var patty = new BurgerPatty();
            return context.CompletedWithVariables(new { patty });
        }

        public Task<CompensationResult> Compensate(CompensateContext<GrillBurgerLog> context)
        {
            throw new NotImplementedException();
        }
    }

    public interface IGrillBurgerArguments
    {
        Guid OrderId { get; }
        decimal Weight { get; }
        decimal WeightTemperature { get; }
        bool Cheese { get; }
    }
    public class GrillBurgerLog
    {
    }
}
