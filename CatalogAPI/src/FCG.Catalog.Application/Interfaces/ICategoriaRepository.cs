using FCG.Catalog.Domain.Entities;

namespace FCG.Catalog.Application.Interfaces;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> ListarAsync();
    Task<Categoria?> BuscarPorIdAsync(int id);
    Task<Categoria> AdicionarAsync(Categoria categoria);
    Task<Categoria> AtualizarAsync(Categoria categoria);
    Task<bool> ExisteAsync(int id);
}
