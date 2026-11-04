using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using FCG.Catalog.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Repositories;

public class BibliotecaRepository : IBibliotecaRepository
{
    private readonly CatalogDbContext _context;

    public BibliotecaRepository(CatalogDbContext context) => _context = context;

    public async Task<IEnumerable<JogoAdquirido>> ListarPorUsuarioAsync(int usuarioId) =>
        await _context.JogosAdquiridos
            .Include(ja => ja.Jogo).ThenInclude(j => j.Categoria)
            .AsNoTracking()
            .Where(ja => ja.UsuarioId == usuarioId)
            .OrderByDescending(ja => ja.DataAquisicao)
            .ToListAsync();

    public async Task<bool> JogoJaAdquiridoAsync(int usuarioId, int jogoId) =>
        await _context.JogosAdquiridos.AnyAsync(ja => ja.UsuarioId == usuarioId && ja.JogoId == jogoId);

    public async Task<JogoAdquirido> AdicionarAsync(JogoAdquirido jogoAdquirido)
    {
        _context.JogosAdquiridos.Add(jogoAdquirido);
        await _context.SaveChangesAsync();
        return jogoAdquirido;
    }
}
