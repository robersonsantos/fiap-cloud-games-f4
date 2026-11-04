using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Controllers;

[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _categoriaService;
    private readonly ILogger<CategoriasController> _logger;

    public CategoriasController(ICategoriaService categoriaService, ILogger<CategoriasController> logger)
    {
        _categoriaService = categoriaService;
        _logger = logger;
    }

    /// <summary>Lista todas as categorias ativas.</summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoriaResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var categorias = await _categoriaService.ListarAsync();
        return Ok(categorias);
    }

    /// <summary>Busca uma categoria pelo Id.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BuscarPorId(int id)
    {
        var categoria = await _categoriaService.BuscarPorIdAsync(id);
        return categoria is null ? NotFound(new { mensagem = $"Categoria com Id {id} não encontrada." }) : Ok(categoria);
    }

    /// <summary>Cria uma nova categoria. Somente Administrador.</summary>
    [HttpPost]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CategoriaNovaDto dto)
    {
        var categoria = await _categoriaService.CriarAsync(dto);
        return CreatedAtAction(nameof(BuscarPorId), new { id = categoria.Id }, categoria);
    }

    /// <summary>Atualiza uma categoria existente. Somente Administrador.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(CategoriaResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] CategoriaNovaDto dto)
    {
        var categoria = await _categoriaService.AtualizarAsync(id, dto);
        return Ok(categoria);
    }

    /// <summary>Remove (soft delete) uma categoria. Somente Administrador.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id)
    {
        await _categoriaService.RemoverAsync(id);
        return NoContent();
    }
}
