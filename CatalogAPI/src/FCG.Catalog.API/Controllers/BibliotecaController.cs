using System.Security.Claims;
using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Controllers;

[ApiController]
[Route("api/biblioteca")]
[Authorize]
[Produces("application/json")]
public class BibliotecaController : ControllerBase
{
    private readonly IBibliotecaService _bibliotecaService;
    private readonly ILogger<BibliotecaController> _logger;

    public BibliotecaController(IBibliotecaService bibliotecaService, ILogger<BibliotecaController> logger)
    {
        _bibliotecaService = bibliotecaService;
        _logger = logger;
    }

    /// <summary>Lista os jogos adquiridos do usuário autenticado.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JogoBibliotecaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarBiblioteca()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (userId == 0)
            return Unauthorized(new { mensagem = "Usuário não identificado no token." });

        var jogos = await _bibliotecaService.ListarBibliotecaAsync(userId);
        return Ok(jogos);
    }
}
