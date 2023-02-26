using MassTransit;
using RoutingSlipSample.Api.Components.Activities.DressBurger;
using RoutingSlipSample.Api.Components.Activities.GrillBurger;
using RoutingSlipSample.Api.Components.Consumers;
using RoutingSlipSample.Contracts;

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

        x.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();
        x.AddActivitiesFromNamespaceContaining<GrillBurgerActivity>();
        x.AddActivitiesFromNamespaceContaining<DressBurgerActivity>();
        x.UsingRabbitMq((context, cfg) =>
        {
            //cfg.SetCustomEntityNameFormatter();

            //cfg.UseRabbitMqMessageScheduler();
            cfg.ConfigureEndpoints(context);
            cfg.Host("rabbitmq://amar:123@localhost:5672");

            var options = new ServiceInstanceOptions();

            cfg.ServiceInstance(options, instance =>
            {
                instance.ConfigureEndpoints(context);
            });

        });
        x.AddRequestClient<SubmitOrder>();
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

app.Run();
