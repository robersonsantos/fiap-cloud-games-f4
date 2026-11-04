using FCG.Payments.Application.Events;
using FCG.Notifications.Application.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Notifications.Application.Consumers;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(INotificationService notificationService, ILogger<PaymentProcessedConsumer> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "[NOTIFICATIONS] Recebido PaymentProcessedEvent: OrderId={OrderId} Status={Status} Email={Email}",
            evt.OrderId, evt.Status, evt.UserEmail);

        try
        {
            if (evt.Status == "Approved")
                await _notificationService.EnviarConfirmacaoCompraAsync(evt.UserEmail, evt.GameName);
            else
                await _notificationService.EnviarRejeicaoCompraAsync(evt.UserEmail, evt.GameName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFICATIONS] Erro ao enviar notificação de pagamento para {Email}", evt.UserEmail);
        }
    }
}
