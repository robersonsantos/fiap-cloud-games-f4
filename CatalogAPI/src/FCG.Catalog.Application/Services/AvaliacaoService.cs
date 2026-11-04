using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Services;

public class AvaliacaoService : IAvaliacaoService
{
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly IJogoRepository _jogoRepository;
    private readonly ILogger<AvaliacaoService> _logger;

    public AvaliacaoService(
        IAvaliacaoRepository avaliacaoRepository,
        IJogoRepository jogoRepository,
        ILogger<AvaliacaoService> logger)
    {
        _avaliacaoRepository = avaliacaoRepository;
        _jogoRepository = jogoRepository;
        _logger = logger;
    }

    public async Task<AvaliacaoResumoDto> ListarPorJogoAsync(int jogoId)
    {
        var jogoExiste = await _jogoRepository.ExisteAsync(jogoId);
        if (!jogoExiste)
            throw new KeyNotFoundException($"Jogo com Id {jogoId} não encontrado.");

        var avaliacoes = (await _avaliacaoRepository.ListarPorJogoAsync(jogoId)).ToList();
        var media = avaliacoes.Count == 0 ? 0 : Math.Round(avaliacoes.Average(a => a.Nota), 2);

        return new AvaliacaoResumoDto(jogoId, media, avaliacoes.Count, avaliacoes.Select(MapToResponseDto));
    }

    public async Task<AvaliacaoResponseDto> AvaliarAsync(int jogoId, int userId, string userEmail, AvaliacaoNovaDto dto)
    {
        if (dto.Nota is < 1 or > 5)
            throw new ArgumentException("A nota deve ser um valor entre 1 e 5.");

        var jogoExiste = await _jogoRepository.ExisteAsync(jogoId);
        if (!jogoExiste)
            throw new KeyNotFoundException($"Jogo com Id {jogoId} não encontrado.");

        // Uma avaliação por usuário por jogo: se já existir, substitui (upsert)
        var existente = await _avaliacaoRepository.BuscarPorJogoEUsuarioAsync(jogoId, userId);

        var avaliacao = new AvaliacaoJogo
        {
            Id = existente?.Id ?? Guid.NewGuid(),
            JogoId = jogoId,
            UserId = userId,
            UserEmail = userEmail,
            Nota = dto.Nota,
            Comentario = dto.Comentario,
            Data = DateTime.UtcNow
        };

        await _avaliacaoRepository.SalvarAsync(avaliacao);

        _logger.LogInformation("[CATALOG] Avaliação registrada no MongoDB: Jogo={JogoId} User={UserId} Nota={Nota}",
            jogoId, userId, dto.Nota);

        return MapToResponseDto(avaliacao);
    }

    public async Task RemoverAsync(int jogoId, int userId)
    {
        var existente = await _avaliacaoRepository.BuscarPorJogoEUsuarioAsync(jogoId, userId)
            ?? throw new KeyNotFoundException($"Avaliação do usuário {userId} para o jogo {jogoId} não encontrada.");

        await _avaliacaoRepository.RemoverAsync(jogoId, userId);
        _logger.LogInformation("[CATALOG] Avaliação removida: Jogo={JogoId} User={UserId}", jogoId, userId);
    }

    private static AvaliacaoResponseDto MapToResponseDto(AvaliacaoJogo a) =>
        new(a.Id, a.JogoId, a.UserId, a.Nota, a.Comentario, a.Data);
}
