using FCG.Notifications.Application.Consumers;
using FCG.Notifications.Application.Services;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserCreatedConsumer>();
    x.AddConsumer<PaymentProcessedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        var host = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq";
        cfg.Host(host, "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]
                ?? throw new InvalidOperationException("RabbitMQ:Username não configurado."));
            h.Password(builder.Configuration["RabbitMQ:Password"]
                ?? throw new InvalidOperationException("RabbitMQ:Password não configurado."));
        });

        cfg.ReceiveEndpoint("fcg.notifications.user.created", e =>
        {
            e.ConfigureConsumer<UserCreatedConsumer>(ctx);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.Bind("fcg.user.created");
        });

        cfg.ReceiveEndpoint("fcg.notifications.payment.processed", e =>
        {
            e.ConfigureConsumer<PaymentProcessedConsumer>(ctx);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.Bind("fcg.payment.processed");
        });
    });
});

var app = builder.Build();
app.Run();
