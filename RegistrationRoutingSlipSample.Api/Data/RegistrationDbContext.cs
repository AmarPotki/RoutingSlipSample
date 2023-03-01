using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;

namespace RegistrationRoutingSlipSample.Api.Data
{
    public class RegistrationDbContext : SagaDbContext
    {
        public RegistrationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override IEnumerable<ISagaClassMap> Configurations { get; } = new[] { new RegistrationStateInstanceMap() };
    }
}
