using FCG.Catalog.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Infrastructure.Messaging;

public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<MassTransitEventPublisher> _logger;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint, ILogger<MassTransitEventPublisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task PublicarAsync<T>(T evento) where T : class
    {
        await _publishEndpoint.Publish(evento);
        _logger.LogInformation("[CATALOG] Evento publicado: {EventType}", typeof(T).Name);
    }
}
