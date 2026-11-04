using System.Security.Claims;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Controllers;

[ApiController]
[Route("api/jogos")]
[Produces("application/json")]
public class JogosController : ControllerBase
{
    private readonly IJogoService _jogoService;
    private readonly IBibliotecaService _bibliotecaService;
    private readonly ILogger<JogosController> _logger;

    public JogosController(IJogoService jogoService, IBibliotecaService bibliotecaService, ILogger<JogosController> logger)
    {
        _jogoService = jogoService;
        _bibliotecaService = bibliotecaService;
        _logger = logger;
    }

    /// <summary>Lista todos os jogos disponíveis.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<JogoResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? busca, [FromQuery] int? categoriaId)
    {
        var jogos = await _jogoService.ListarAsync(busca, categoriaId);
        return Ok(jogos);
    }

    /// <summary>Busca um jogo pelo Id.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JogoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var jogo = await _jogoService.BuscarPorIdAsync(id);
        return jogo is null ? NotFound(new { mensagem = $"Jogo com Id {id} não encontrado." }) : Ok(jogo);
    }

    /// <summary>Cria um novo jogo. Somente Administrador.</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(JogoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] JogoNovoDto dto)
    {
        var jogo = await _jogoService.CriarAsync(dto);
        return CreatedAtAction(nameof(BuscarPorId), new { id = jogo.Id }, jogo);
    }

    /// <summary>Atualiza um jogo existente. Somente Administrador.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(JogoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] JogoAtualizarDto dto)
    {
        var jogo = await _jogoService.AtualizarAsync(id, dto);
        return Ok(jogo);
    }

    /// <summary>Remove (soft delete) um jogo. Somente Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        await _jogoService.RemoverAsync(id);
        return NoContent();
    }

    /// <summary>Inicia o processo de compra de um jogo.</summary>
    [HttpPost("{id:int}/comprar")]
    [Authorize]
    [ProducesResponseType(typeof(CompraResponseDto), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Comprar(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

        if (userId == 0)
            return Unauthorized(new { mensagem = "Usuário não identificado no token." });

        var resultado = await _bibliotecaService.IniciarCompraAsync(id, userId, userEmail);
        return Accepted(resultado);
    }
}
