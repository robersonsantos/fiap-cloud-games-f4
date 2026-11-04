using System.Diagnostics;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Services;

public class BuscaService : IBuscaService
{
    public const int TamanhoPadrao = 10;
    public const int TamanhoMaximo = 50;

    private readonly IJogoSearchIndex _searchIndex;
    private readonly IJogoRepository _jogoRepository;
    private readonly ILogger<BuscaService> _logger;

    public BuscaService(IJogoSearchIndex searchIndex, IJogoRepository jogoRepository, ILogger<BuscaService> logger)
    {
        _searchIndex = searchIndex;
        _jogoRepository = jogoRepository;
        _logger = logger;
    }

    public async Task<BuscaResponseDto> BuscarAsync(string? termo, int? tamanho, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(termo))
            throw new ArgumentException("Informe o termo de busca no parâmetro 'q'.");

        var termoNormalizado = termo.Trim();
        var tamanhoEfetivo = Math.Clamp(tamanho ?? TamanhoPadrao, 1, TamanhoMaximo);

        var sw = Stopwatch.StartNew();
        var resultados = await _searchIndex.BuscarAsync(termoNormalizado, tamanhoEfetivo, ct);
        sw.Stop();

        _logger.LogInformation("[CATALOG] [SEARCH] '{Termo}' → {Total} resultado(s) em {Ms}ms (OpenSearch).",
            termoNormalizado, resultados.Count, sw.ElapsedMilliseconds);

        return new BuscaResponseDto(termoNormalizado, resultados.Count, sw.ElapsedMilliseconds, resultados);
    }

    public async Task<ReindexacaoResponseDto> ReindexarAsync(CancellationToken ct = default)
    {
        await _searchIndex.GarantirIndiceAsync(ct);

        var jogos = await _jogoRepository.ListarAsync(null, null, apenasAtivosVisiveis: false);
        var documentos = jogos.Select(ParaDocumento).ToList();

        await _searchIndex.IndexarLoteAsync(documentos, ct);
        _logger.LogInformation("[CATALOG] [SEARCH] Reindexação concluída: {Total} jogo(s).", documentos.Count);

        return new ReindexacaoResponseDto(documentos.Count);
    }

    public static JogoDocumentoBusca ParaDocumento(Jogo jogo) =>
        new(jogo.Id, jogo.Nome, jogo.Descricao, jogo.Preco,
            jogo.Categoria?.Nome ?? string.Empty, jogo.EstaDisponivel(), jogo.DataCadastro);
}
