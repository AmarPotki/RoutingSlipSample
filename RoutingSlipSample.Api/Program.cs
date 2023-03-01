using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using RoutingSlipSample.Api.Components.Activities.DressBurger;
using RoutingSlipSample.Api.Components.Activities.GrillBurger;
using RoutingSlipSample.Api.Components.Consumers;
using RoutingSlipSample.Contracts;
using Newtonsoft.Json.Linq;
using RoutingSlipSample.Api.Extensions;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();
        // x.AddRabbitMqMessageScheduler();
        x.ApplyCustomMassTransitConfiguration();
        x.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
        x.AddConsumersFromNamespaceContaining<SubmitOrderWaitUntilRoutingSlipFinishConsumer>();
        x.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();
        x.AddActivitiesFromNamespaceContaining<DressBurgerActivity>();
        x.UsingRabbitMq((context, cfg) =>
        {
            //cfg.SetCustomEntityNameFormatter();

            //cfg.UseRabbitMqMessageScheduler();
            cfg.ConfigureEndpoints(context);
            cfg.Host("rabbitmq://amar:123@localhost:5672");
            cfg.AutoStart = true;

            var options = new ServiceInstanceOptions();

            cfg.ServiceInstance(options, instance =>
            {
                instance.ConfigureEndpoints(context);
            });

        });
        x.AddRequestClient<SubmitOrder>();
    });

builder.Host.UseSerilog((host, log) =>
{
    if (host.HostingEnvironment.IsProduction())
        log.MinimumLevel.Information();
    else
        log.MinimumLevel.Debug();
    log.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
    log.MinimumLevel.Override("Quartz", LogEventLevel.Information);
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