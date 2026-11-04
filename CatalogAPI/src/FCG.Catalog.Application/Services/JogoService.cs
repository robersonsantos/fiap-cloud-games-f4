using System.Text.Json;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Services;

public class JogoService : IJogoService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private const string CacheVersionKey = "jogos:cache-version";

    private readonly IJogoRepository _jogoRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IDistributedCache _cache;
    private readonly IJogoSearchIndex _searchIndex;
    private readonly ILogger<JogoService> _logger;

    public JogoService(
        IJogoRepository jogoRepository,
        ICategoriaRepository categoriaRepository,
        IDistributedCache cache,
        IJogoSearchIndex searchIndex,
        ILogger<JogoService> logger)
    {
        _jogoRepository = jogoRepository;
        _categoriaRepository = categoriaRepository;
        _cache = cache;
        _searchIndex = searchIndex;
        _logger = logger;
    }

    public async Task<IEnumerable<JogoResponseDto>> ListarAsync(string? busca, int? categoriaId)
    {
        var cacheKey = $"jogos:lista:v{await ObterVersaoCacheAsync()}:{busca ?? "*"}:{categoriaId?.ToString() ?? "*"}";

        var emCache = await CacheGetAsync<List<JogoResponseDto>>(cacheKey);
        if (emCache is not null)
        {
            _logger.LogInformation("[CATALOG] [CACHE HIT] Listagem de jogos servida do Redis: {Key}", cacheKey);
            return emCache;
        }

        var jogos = await _jogoRepository.ListarAsync(busca, categoriaId, apenasAtivosVisiveis: true);
        var resultado = jogos.Select(MapToResponseDto).ToList();

        await CacheSetAsync(cacheKey, resultado);
        _logger.LogInformation("[CATALOG] [CACHE MISS] Listagem de jogos consultada no SQL Server e armazenada no Redis: {Key}", cacheKey);

        return resultado;
    }

    public async Task<JogoResponseDto?> BuscarPorIdAsync(int id)
    {
        var cacheKey = $"jogos:item:{id}";

        var emCache = await CacheGetAsync<JogoResponseDto>(cacheKey);
        if (emCache is not null)
        {
            _logger.LogInformation("[CATALOG] [CACHE HIT] Jogo {Id} servido do Redis.", id);
            return emCache;
        }

        var jogo = await _jogoRepository.BuscarPorIdAsync(id);
        if (jogo is null)
            return null;

        var dto = MapToResponseDto(jogo);
        await CacheSetAsync(cacheKey, dto);
        return dto;
    }

    public async Task<JogoResponseDto> CriarAsync(JogoNovoDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new ArgumentException("Nome do jogo é obrigatório.");

        var categoriaExiste = await _categoriaRepository.ExisteAsync(dto.CategoriaId);
        if (!categoriaExiste)
            throw new KeyNotFoundException($"Categoria com Id {dto.CategoriaId} não encontrada.");

        var jogo = new Jogo
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            CategoriaId = dto.CategoriaId,
            Ativo = true,
            Visivel = true,
            DataCadastro = DateTime.UtcNow
        };

        var criado = await _jogoRepository.AdicionarAsync(jogo);
        await InvalidarCacheAsync(criado.Id);
        await SincronizarIndiceAsync(criado);
        _logger.LogInformation("[CATALOG] Jogo criado: {Id} - {Nome}", criado.Id, criado.Nome);
        return MapToResponseDto(criado);
    }

    public async Task<JogoResponseDto> AtualizarAsync(int id, JogoAtualizarDto dto)
    {
        var jogo = await _jogoRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Jogo com Id {id} não encontrado.");

        var categoriaExiste = await _categoriaRepository.ExisteAsync(dto.CategoriaId);
        if (!categoriaExiste)
            throw new KeyNotFoundException($"Categoria com Id {dto.CategoriaId} não encontrada.");

        jogo.Atualizar(dto.Nome, dto.Descricao, dto.Preco, dto.CategoriaId, dto.Visivel);

        var atualizado = await _jogoRepository.AtualizarAsync(jogo);
        await InvalidarCacheAsync(id);
        await SincronizarIndiceAsync(atualizado);
        _logger.LogInformation("[CATALOG] Jogo atualizado: {Id} - {Nome}", atualizado.Id, atualizado.Nome);
        return MapToResponseDto(atualizado);
    }

    public async Task RemoverAsync(int id)
    {
        var jogo = await _jogoRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Jogo com Id {id} não encontrado.");

        jogo.Desativar();
        await _jogoRepository.AtualizarAsync(jogo);
        await InvalidarCacheAsync(id);
        await RemoverDoIndiceAsync(id);
        _logger.LogInformation("[CATALOG] Jogo removido (soft delete): {Id} - {Nome}", jogo.Id, jogo.Nome);
    }

    // ─── Busca (OpenSearch) ────────────────────────────────────────────────────
    // O SQL Server é a fonte da verdade. Uma falha no OpenSearch não desfaz a escrita:
    // ela é registrada e pode ser corrigida por POST /api/jogos/search/reindexar.

    private async Task SincronizarIndiceAsync(Jogo jogo)
    {
        try
        {
            await _searchIndex.IndexarAsync(BuscaService.ParaDocumento(jogo));
            _logger.LogInformation("[CATALOG] [SEARCH] Jogo {Id} indexado no OpenSearch.", jogo.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CATALOG] [SEARCH] Falha ao indexar o jogo {Id} no OpenSearch.", jogo.Id);
        }
    }

    private async Task RemoverDoIndiceAsync(int jogoId)
    {
        try
        {
            await _searchIndex.RemoverAsync(jogoId);
            _logger.LogInformation("[CATALOG] [SEARCH] Jogo {Id} removido do índice do OpenSearch.", jogoId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CATALOG] [SEARCH] Falha ao remover o jogo {Id} do OpenSearch.", jogoId);
        }
    }

    private static JogoResponseDto MapToResponseDto(Jogo jogo) =>
        new(jogo.Id, jogo.Nome, jogo.Descricao, jogo.Preco,
            jogo.Categoria?.Nome ?? string.Empty, jogo.Ativo, jogo.Visivel);

    // ─── Cache (Redis via IDistributedCache) ────────────────────────────────────
    // As chaves de listagem embutem uma "versão"; ao mutar o catálogo a versão é
    // incrementada, invalidando todas as listagens de uma só vez (o IDistributedCache
    // não permite varrer chaves). Falhas no Redis não derrubam a operação: a API
    // degrada para consultar direto o banco.

    private async Task<long> ObterVersaoCacheAsync()
    {
        var versao = await CacheGetAsync<long?>(CacheVersionKey);
        return versao ?? 0;
    }

    private async Task InvalidarCacheAsync(int jogoId)
    {
        try
        {
            var novaVersao = await ObterVersaoCacheAsync() + 1;
            await _cache.SetStringAsync(CacheVersionKey, JsonSerializer.Serialize<long?>(novaVersao));
            await _cache.RemoveAsync($"jogos:item:{jogoId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CATALOG] Falha ao invalidar cache no Redis.");
        }
    }

    private async Task<T?> CacheGetAsync<T>(string key)
    {
        try
        {
            var json = await _cache.GetStringAsync(key);
            return json is null ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CATALOG] Falha ao ler cache do Redis (chave {Key}).", key);
            return default;
        }
    }

    private async Task CacheSetAsync<T>(string key, T valor)
    {
        try
        {
            await _cache.SetStringAsync(key, JsonSerializer.Serialize(valor), CacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[CATALOG] Falha ao gravar cache no Redis (chave {Key}).", key);
        }
    }
}
