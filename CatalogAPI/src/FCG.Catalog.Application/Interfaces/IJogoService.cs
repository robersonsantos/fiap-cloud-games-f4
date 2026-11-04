using FCG.Catalog.Application.DTOs;

namespace FCG.Catalog.Application.Interfaces;

public interface IJogoService
{
    Task<IEnumerable<JogoResponseDto>> ListarAsync(string? busca, int? categoriaId);
    Task<JogoResponseDto?> BuscarPorIdAsync(int id);
    Task<JogoResponseDto> CriarAsync(JogoNovoDto dto);
    Task<JogoResponseDto> AtualizarAsync(int id, JogoAtualizarDto dto);
    Task RemoverAsync(int id);
}
