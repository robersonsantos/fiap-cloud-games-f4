using System.Security.Claims;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Controllers;

[ApiController]
[Route("api/jogos/{jogoId:int}/avaliacoes")]
[Produces("application/json")]
public class AvaliacoesController : ControllerBase
{
    private readonly IAvaliacaoService _avaliacaoService;

    public AvaliacoesController(IAvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    /// <summary>Lista as avaliações de um jogo, com média e total (dados no MongoDB).</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AvaliacaoResumoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Listar(int jogoId)
    {
        var resumo = await _avaliacaoService.ListarPorJogoAsync(jogoId);
        return Ok(resumo);
    }

    /// <summary>Registra (ou substitui) a avaliação do usuário autenticado para o jogo.</summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(AvaliacaoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Avaliar(int jogoId, [FromBody] AvaliacaoNovaDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        if (userId == 0)
            return Unauthorized(new { mensagem = "Usuário não identificado no token." });

        var avaliacao = await _avaliacaoService.AvaliarAsync(jogoId, userId, userEmail, dto);
        return CreatedAtAction(nameof(Listar), new { jogoId }, avaliacao);
    }

    /// <summary>Remove a avaliação do usuário autenticado para o jogo.</summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int jogoId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (userId == 0)
            return Unauthorized(new { mensagem = "Usuário não identificado no token." });

        await _avaliacaoService.RemoverAsync(jogoId, userId);
        return NoContent();
    }
}
