using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Interfaces;

public interface IJogoRepository
{
    Task<IEnumerable<Jogo>> ListarAsync(string? busca, int? categoriaId, bool apenasAtivosVisiveis = true);
    Task<Jogo?> BuscarPorIdAsync(int id);
    Task<Jogo> AdicionarAsync(Jogo jogo);
    Task<Jogo> AtualizarAsync(Jogo jogo);
    Task<bool> ExisteAsync(int id);
}
