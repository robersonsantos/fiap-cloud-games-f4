using FCG.Catalog.Application.DTOs;

namespace FCG.Catalog.Application.Interfaces;

public interface ICategoriaService
{
    Task<IEnumerable<CategoriaResponseDto>> ListarAsync();
    Task<CategoriaResponseDto?> BuscarPorIdAsync(int id);
    Task<CategoriaResponseDto> CriarAsync(CategoriaNovaDto dto);
    Task<CategoriaResponseDto> AtualizarAsync(int id, CategoriaNovaDto dto);
    Task RemoverAsync(int id);
}
