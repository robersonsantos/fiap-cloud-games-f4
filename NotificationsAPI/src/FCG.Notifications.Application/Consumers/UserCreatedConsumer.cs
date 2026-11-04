using FCG.Users.Application.Events;
using FCG.Notifications.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Application.Consumers;

public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<UserCreatedConsumer> _logger;

    public UserCreatedConsumer(INotificationService notificationService, ILogger<UserCreatedConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("[NOTIFICATIONS] Recebido UserCreatedEvent: UserId={UserId} Email={Email}",
            evt.UserId, evt.Email);

        try
        {
            await _notificationService.EnviarBoasVindasAsync(evt.Email, evt.Nome);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFICATIONS] Erro ao enviar e-mail de boas-vindas para {Email}", evt.Email);
        }
    }
}
