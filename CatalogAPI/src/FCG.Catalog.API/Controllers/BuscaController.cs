using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Controllers;

/// <summary>Busca avançada no catálogo via Amazon OpenSearch (Fase 4).</summary>
[ApiController]
[Produces("application/json")]
public class BuscaController : ControllerBase
{
    private readonly IBuscaService _buscaService;

    public BuscaController(IBuscaService buscaService) => _buscaService = buscaService;

    /// <summary>
    /// Busca jogos por nome, categoria ou descrição com tolerância a erros de digitação
    /// (fuzzy) e ordenação por relevância.
    /// </summary>
    /// <param name="q">Termo de busca (ex.: "zelad", "acao").</param>
    /// <param name="tamanho">Quantidade máxima de resultados (1 a 50, padrão 10).</param>
    [HttpGet("/search")]
    [HttpGet("/api/jogos/search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BuscaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Buscar([FromQuery] string? q, [FromQuery] int? tamanho, CancellationToken ct)
    {
        var resultado = await _buscaService.BuscarAsync(q, tamanho, ct);
        return Ok(resultado);
    }

    /// <summary>Reconstrói o índice de busca a partir do SQL Server. Somente Administrador.</summary>
    [HttpPost("/api/jogos/search/reindexar")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(ReindexacaoResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reindexar(CancellationToken ct)
    {
        var resultado = await _buscaService.ReindexarAsync(ct);
        return Ok(resultado);
    }
}
