using FCG.Payments.Application.Consumers;
using FCG.Payments.Application.Interfaces;
using FCG.Payments.Application.Services;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<PaymentSettings>(
    builder.Configuration.GetSection("Payment"));

builder.Services.AddScoped<IPaymentSimulatorService, PaymentSimulatorService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderPlacedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        var username = builder.Configuration["RabbitMQ:Username"]
            ?? throw new InvalidOperationException("RabbitMQ:Username não configurado.");
        var password = builder.Configuration["RabbitMQ:Password"]
            ?? throw new InvalidOperationException("RabbitMQ:Password não configurado.");

        cfg.Host(host, "/", h =>
        {
            h.Username(username);
            h.Password(password);
        });

        cfg.ReceiveEndpoint("fcg.order.placed", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<OrderPlacedConsumer>(ctx);
        });

        cfg.Message<FCG.Payments.Application.Events.PaymentProcessedEvent>(m =>
            m.SetEntityName("fcg.payment.processed"));
    });
});

var app = builder.Build();
app.Run();
