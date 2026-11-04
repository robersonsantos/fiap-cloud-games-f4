using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Events;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Services;

public class BibliotecaService : IBibliotecaService
{
    private readonly IBibliotecaRepository _bibliotecaRepository;
    private readonly IJogoRepository _jogoRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<BibliotecaService> _logger;

    public BibliotecaService(
        IBibliotecaRepository bibliotecaRepository,
        IJogoRepository jogoRepository,
        IEventPublisher eventPublisher,
        ILogger<BibliotecaService> logger)
    {
        _bibliotecaRepository = bibliotecaRepository;
        _jogoRepository = jogoRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CompraResponseDto> IniciarCompraAsync(int jogoId, int userId, string userEmail)
    {
        var jogo = await _jogoRepository.BuscarPorIdAsync(jogoId)
            ?? throw new KeyNotFoundException($"Jogo com Id {jogoId} não encontrado.");

        if (!jogo.EstaDisponivel())
            throw new InvalidOperationException($"Jogo '{jogo.Nome}' não está disponível para compra.");

        var jaAdquirido = await _bibliotecaRepository.JogoJaAdquiridoAsync(userId, jogoId);
        if (jaAdquirido)
            throw new InvalidOperationException($"Jogo '{jogo.Nome}' já está na sua biblioteca.");

        var orderId = Guid.NewGuid();
        var evento = new OrderPlacedEvent(orderId, userId, jogo.Id, jogo.Nome, jogo.Preco, userEmail);
        await _eventPublisher.PublicarAsync(evento);

        _logger.LogInformation("[CATALOG] Compra iniciada. OrderId: {OrderId}, UserId: {UserId}, GameId: {GameId}",
            orderId, userId, jogoId);

        return new CompraResponseDto(orderId, $"Pedido {orderId} criado. Aguardando processamento do pagamento.");
    }

    public async Task AdicionarJogoBibliotecaAsync(int userId, int jogoId, decimal valorPago, Guid transacaoId)
    {
        var jogoAdquirido = new JogoAdquirido
        {
            UsuarioId = userId,
            JogoId = jogoId,
            ValorPago = valorPago,
            DataAquisicao = DateTime.UtcNow,
            TransacaoId = transacaoId
        };

        await _bibliotecaRepository.AdicionarAsync(jogoAdquirido);
        _logger.LogInformation("[CATALOG] Jogo {JogoId} adicionado à biblioteca do usuário {UserId}. TransacaoId: {TransacaoId}",
            jogoId, userId, transacaoId);
    }

    public async Task<IEnumerable<JogoBibliotecaDto>> ListarBibliotecaAsync(int userId)
    {
        var jogosAdquiridos = await _bibliotecaRepository.ListarPorUsuarioAsync(userId);
        return jogosAdquiridos.Select(ja => new JogoBibliotecaDto(
            ja.Jogo.Id,
            ja.Jogo.Nome,
            ja.Jogo.Descricao,
            ja.Jogo.Categoria?.Nome ?? string.Empty,
            ja.ValorPago,
            ja.DataAquisicao));
    }
}
