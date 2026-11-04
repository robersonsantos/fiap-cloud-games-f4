using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class JogoRepository : IJogoRepository
{
    private readonly CatalogDbContext _context;

    public JogoRepository(CatalogDbContext context) => _context = context;

    public async Task<IEnumerable<Jogo>> ListarAsync(string? busca, int? categoriaId, bool apenasAtivosVisiveis = true)
    {
        var query = _context.Jogos.Include(j => j.Categoria).AsNoTracking();

        if (apenasAtivosVisiveis)
            query = query.Where(j => j.Ativo && j.Visivel);

        if (!string.IsNullOrWhiteSpace(busca))
            query = query.Where(j => j.Nome.Contains(busca) || j.Descricao.Contains(busca));

        if (categoriaId.HasValue)
            query = query.Where(j => j.CategoriaId == categoriaId.Value);

        return await query.OrderBy(j => j.Nome).ToListAsync();
    }

    public async Task<Jogo?> BuscarPorIdAsync(int id) =>
        await _context.Jogos.Include(j => j.Categoria).FirstOrDefaultAsync(j => j.Id == id);

    public async Task<Jogo> AdicionarAsync(Jogo jogo)
    {
        _context.Jogos.Add(jogo);
        await _context.SaveChangesAsync();
        await _context.Entry(jogo).Reference(j => j.Categoria).LoadAsync();
        return jogo;
    }

    public async Task<Jogo> AtualizarAsync(Jogo jogo)
    {
        _context.Jogos.Update(jogo);
        await _context.SaveChangesAsync();
        await _context.Entry(jogo).Reference(j => j.Categoria).LoadAsync();
        return jogo;
    }

    public async Task<bool> ExisteAsync(int id) =>
        await _context.Jogos.AnyAsync(j => j.Id == id);
}
