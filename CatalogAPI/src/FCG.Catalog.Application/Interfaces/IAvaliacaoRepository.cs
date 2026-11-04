using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Interfaces;

public interface IAvaliacaoRepository
{
    Task<IEnumerable<AvaliacaoJogo>> ListarPorJogoAsync(int jogoId);
    Task<AvaliacaoJogo?> BuscarPorJogoEUsuarioAsync(int jogoId, int userId);
    Task SalvarAsync(AvaliacaoJogo avaliacao);
    Task RemoverAsync(int jogoId, int userId);
}
