using FCG.Catalog.Application.DTOs;

namespace FCG.Catalog.Application.Interfaces;

public interface IAvaliacaoService
{
    Task<AvaliacaoResumoDto> ListarPorJogoAsync(int jogoId);
    Task<AvaliacaoResponseDto> AvaliarAsync(int jogoId, int userId, string userEmail, AvaliacaoNovaDto dto);
    Task RemoverAsync(int jogoId, int userId);
}
