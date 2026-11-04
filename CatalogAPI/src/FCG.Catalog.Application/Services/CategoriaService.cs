using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace FCG.Catalog.Application.Services;

public class CategoriaService : ICategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly ILogger<CategoriaService> _logger;

    public CategoriaService(ICategoriaRepository categoriaRepository, ILogger<CategoriaService> logger)
    {
        _categoriaRepository = categoriaRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<CategoriaResponseDto>> ListarAsync()
    {
        var categorias = await _categoriaRepository.ListarAsync();
        return categorias.Select(MapToResponseDto);
    }

    public async Task<CategoriaResponseDto?> BuscarPorIdAsync(int id)
    {
        var categoria = await _categoriaRepository.BuscarPorIdAsync(id);
        return categoria is null ? null : MapToResponseDto(categoria);
    }

    public async Task<CategoriaResponseDto> CriarAsync(CategoriaNovaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new ArgumentException("Nome da categoria é obrigatório.");

        var categoria = new Categoria
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Ativo = true
        };

        var criada = await _categoriaRepository.AdicionarAsync(categoria);
        _logger.LogInformation("[CATALOG] Categoria criada: {Id} - {Nome}", criada.Id, criada.Nome);
        return MapToResponseDto(criada);
    }

    public async Task<CategoriaResponseDto> AtualizarAsync(int id, CategoriaNovaDto dto)
    {
        var categoria = await _categoriaRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Categoria com Id {id} não encontrada.");

        categoria.Atualizar(dto.Nome, dto.Descricao);

        var atualizada = await _categoriaRepository.AtualizarAsync(categoria);
        _logger.LogInformation("[CATALOG] Categoria atualizada: {Id} - {Nome}", atualizada.Id, atualizada.Nome);
        return MapToResponseDto(atualizada);
    }

    public async Task RemoverAsync(int id)
    {
        var categoria = await _categoriaRepository.BuscarPorIdAsync(id)
            ?? throw new KeyNotFoundException($"Categoria com Id {id} não encontrada.");

        categoria.Desativar();
        await _categoriaRepository.AtualizarAsync(categoria);
        _logger.LogInformation("[CATALOG] Categoria removida (soft delete): {Id} - {Nome}", categoria.Id, categoria.Nome);
    }

    private static CategoriaResponseDto MapToResponseDto(Categoria c) =>
        new(c.Id, c.Nome, c.Descricao, c.Ativo);
}
