using System.Reflection;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json.Linq;
using RegistrationRoutingSlipSample.Api.Components;
using RegistrationRoutingSlipSample.Api.Components.Activities;
using RegistrationRoutingSlipSample.Api.Components.Consumers;
using RegistrationRoutingSlipSample.Api.Components.StateMachines;
using RegistrationRoutingSlipSample.Api.Data;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RegistrationDbContext>(r =>
{
    var connectionString = "Server=192.168.10.140;Database=edaSagaRegistration;Persist Security Info=False;User ID=sa;Password=Ai@123456;Encrypt=False;TrustServerCertificate=True;"
        ;

    r.UseSqlServer(connectionString, m =>
    {
        m.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
        m.MigrationsHistoryTable($"__{nameof(RegistrationDbContext)}");
    });
});

builder.Services.AddSingleton<IEndpointAddressProvider, RabbitMqEndpointAddressProvider>();
builder.Services.AddTransient<ISecurePaymentInfoService, SecurePaymentInfoService>();
builder.Services.AddMassTransit(x =>
{
    x.SetEntityFrameworkSagaRepositoryProvider(r =>
    {
        r.ExistingDbContext<RegistrationDbContext>();
        r.UseSqlServer();
    });

    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<ProcessRegistrationConsumer>();
    x.AddConsumer<SubmitRegistrationConsumer>();
    x.AddExecuteActivity<LicenseVerificationActivity,LicenseVerificationArguments>();
    x.AddActivity<EventRegistrationActivity, EventRegistrationArguments, EventRegistrationLog>();
    x.AddActivity<ProcessPaymentActivity, ProcessPaymentArguments,ProcessPaymentLog>();
    x.AddExecuteActivity<AssignWaiverActivity, AssignWaiverArguments>();
    x.AddSagaStateMachine<RegistrationStateMachine, RegistrationState>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.UseDelayedMessageScheduler();
        cfg.ConfigureEndpoints(context);
        cfg.Host("rabbitmq://amar:123@localhost:5672");
        cfg.AutoStart = true;

        var options = new ServiceInstanceOptions();

        cfg.ServiceInstance(options, instance =>
        {
            instance.ConfigureEndpoints(context);
        });
    });
});
builder.Host.UseSerilog((host, log) =>
{
    if (host.HostingEnvironment.IsProduction())
        log.MinimumLevel.Information();
    else
        log.MinimumLevel.Debug();
    log.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
    log.MinimumLevel.Override("Quartz", LogEventLevel.Information);
    log.MinimumLevel.Override("Microsoft.Hosting", LogEventLevel.Information);
    log.MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning);
    log.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning);
    log.MinimumLevel.Override("MassTransit", LogEventLevel.Information);
    log.WriteTo.Console();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter
});

app.MapHealthChecks("/health/live", new HealthCheckOptions { ResponseWriter = HealthCheckResponseWriter });
app.Run();


static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
{
    var json = new JObject(
        new JProperty("status", result.Status.ToString()),
        new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
            new JProperty("status", entry.Value.Status.ToString()),
            new JProperty("description", entry.Value.Description),
            new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

    context.Response.ContentType = "application/json";

    return context.Response.WriteAsync(json.ToString());
}
