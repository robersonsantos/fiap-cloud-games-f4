using FCG.Catalog.Application.DTOs;

namespace FCG.Catalog.Application.Interfaces;

public interface IBuscaService
{
    Task<BuscaResponseDto> BuscarAsync(string? termo, int? tamanho, CancellationToken ct = default);
    Task<ReindexacaoResponseDto> ReindexarAsync(CancellationToken ct = default);
}
