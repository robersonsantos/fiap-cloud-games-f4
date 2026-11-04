using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Interfaces;

public interface IBibliotecaRepository
{
    Task<IEnumerable<JogoAdquirido>> ListarPorUsuarioAsync(int usuarioId);
    Task<bool> JogoJaAdquiridoAsync(int usuarioId, int jogoId);
    Task<JogoAdquirido> AdicionarAsync(JogoAdquirido jogoAdquirido);
}
