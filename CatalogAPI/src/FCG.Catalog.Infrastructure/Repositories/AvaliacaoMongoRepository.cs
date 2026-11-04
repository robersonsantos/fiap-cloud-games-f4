using FCG.Catalog.Application.Interfaces;
using FCG.Catalog.Domain.Entities;
using MongoDB.Driver;

namespace FCG.Catalog.Infrastructure.Repositories;

/// <summary>
/// Repositório de avaliações persistidas no MongoDB (persistência poliglota — Fase 3).
/// Usa o driver oficial MongoDB.Driver.
/// </summary>
public class AvaliacaoMongoRepository : IAvaliacaoRepository
{
    private const string CollectionName = "avaliacoes";
    private readonly IMongoCollection<AvaliacaoJogo> _collection;

    public AvaliacaoMongoRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<AvaliacaoJogo>(CollectionName);
    }

    public async Task<IEnumerable<AvaliacaoJogo>> ListarPorJogoAsync(int jogoId)
    {
        return await _collection
            .Find(a => a.JogoId == jogoId)
            .SortByDescending(a => a.Data)
            .ToListAsync();
    }

    public async Task<AvaliacaoJogo?> BuscarPorJogoEUsuarioAsync(int jogoId, int userId)
    {
        return await _collection
            .Find(a => a.JogoId == jogoId && a.UserId == userId)
            .FirstOrDefaultAsync();
    }

    public async Task SalvarAsync(AvaliacaoJogo avaliacao)
    {
        await _collection.ReplaceOneAsync(
            a => a.JogoId == avaliacao.JogoId && a.UserId == avaliacao.UserId,
            avaliacao,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task RemoverAsync(int jogoId, int userId)
    {
        await _collection.DeleteOneAsync(a => a.JogoId == jogoId && a.UserId == userId);
    }
}
