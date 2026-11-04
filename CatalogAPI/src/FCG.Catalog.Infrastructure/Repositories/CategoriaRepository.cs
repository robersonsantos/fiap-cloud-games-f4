using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly CatalogDbContext _context;

    public CategoriaRepository(CatalogDbContext context) => _context = context;

    public async Task<IEnumerable<Categoria>> ListarAsync() =>
        await _context.Categorias.AsNoTracking().Where(c => c.Ativo).OrderBy(c => c.Nome).ToListAsync();

    public async Task<Categoria?> BuscarPorIdAsync(int id) =>
        await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Categoria> AdicionarAsync(Categoria categoria)
    {
        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<Categoria> AtualizarAsync(Categoria categoria)
    {
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
        return categoria;
    }

    public async Task<bool> ExisteAsync(int id) =>
        await _context.Categorias.AnyAsync(c => c.Id == id && c.Ativo);
}
