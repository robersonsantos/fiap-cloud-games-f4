using FCG.Payments.Application.Events;
using FCG.Catalog.Application.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Infrastructure.Messaging;

public class PaymentProcessedConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly IBibliotecaService _bibliotecaService;
    private readonly IJogoRepository _jogoRepository;
    private readonly ILogger<PaymentProcessedConsumer> _logger;

    public PaymentProcessedConsumer(
        IBibliotecaService bibliotecaService,
        IJogoRepository jogoRepository,
        ILogger<PaymentProcessedConsumer> logger)
    {
        _bibliotecaService = bibliotecaService;
        _jogoRepository = jogoRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("[CATALOG] Pagamento {Status} para OrderId {OrderId}, UserId {UserId}, GameId {GameId}",
            evt.Status, evt.OrderId, evt.UserId, evt.GameId);

        if (evt.Status == "Approved")
        {
            var jogo = await _jogoRepository.BuscarPorIdAsync(evt.GameId);
            var preco = jogo?.Preco ?? 0m;

            await _bibliotecaService.AdicionarJogoBibliotecaAsync(
                evt.UserId, evt.GameId, preco, evt.OrderId);

            _logger.LogInformation("[CATALOG] Jogo {GameId} adicionado à biblioteca do usuário {UserId} via evento.",
                evt.GameId, evt.UserId);
        }
        else
        {
            _logger.LogWarning("[CATALOG] Pagamento não aprovado para OrderId {OrderId}. Status: {Status}",
                evt.OrderId, evt.Status);
        }
    }
}
