using FCG.Catalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG.Catalog.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Jogo> Jogos => Set<Jogo>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<JogoAdquirido> JogosAdquiridos => Set<JogoAdquirido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Descricao).HasMaxLength(500);
            entity.Property(c => c.Ativo).HasDefaultValue(true);
        });

        modelBuilder.Entity<Jogo>(entity =>
        {
            entity.HasKey(j => j.Id);
            entity.Property(j => j.Nome).IsRequired().HasMaxLength(200);
            entity.Property(j => j.Descricao).HasMaxLength(1000);
            entity.Property(j => j.Preco).HasColumnType("decimal(18,2)");
            entity.Property(j => j.Ativo).HasDefaultValue(true);
            entity.Property(j => j.Visivel).HasDefaultValue(true);
            entity.Property(j => j.DataCadastro).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(j => j.Categoria)
                  .WithMany(c => c.Jogos)
                  .HasForeignKey(j => j.CategoriaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JogoAdquirido>(entity =>
        {
            entity.HasKey(ja => ja.Id);
            entity.Property(ja => ja.ValorPago).HasColumnType("decimal(18,2)");
            entity.Property(ja => ja.DataAquisicao).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(ja => ja.TransacaoId).IsRequired();
            entity.HasOne(ja => ja.Jogo)
                  .WithMany(j => j.JogosAdquiridos)
                  .HasForeignKey(ja => ja.JogoId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(ja => new { ja.UsuarioId, ja.JogoId }).IsUnique();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, Nome = "Ação", Descricao = "Jogos de ação e combate", Ativo = true },
            new Categoria { Id = 2, Nome = "Aventura", Descricao = "Jogos de exploração e aventura", Ativo = true },
            new Categoria { Id = 3, Nome = "RPG", Descricao = "Role-Playing Games", Ativo = true },
            new Categoria { Id = 4, Nome = "Estratégia", Descricao = "Jogos de estratégia e táticas", Ativo = true },
            new Categoria { Id = 5, Nome = "Esportes", Descricao = "Simuladores esportivos", Ativo = true }
        );

        modelBuilder.Entity<Jogo>().HasData(
            new Jogo { Id = 1, Nome = "Cyber Warriors", Descricao = "Batalha futurista entre guerreiros cibernéticos", Preco = 59.90m, CategoriaId = 1, Ativo = true, Visivel = true, DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Jogo { Id = 2, Nome = "Lost Kingdom", Descricao = "Explore um reino perdido repleto de mistérios", Preco = 49.90m, CategoriaId = 2, Ativo = true, Visivel = true, DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Jogo { Id = 3, Nome = "Dragon Quest Online", Descricao = "RPG massivo com dragões e magias poderosas", Preco = 79.90m, CategoriaId = 3, Ativo = true, Visivel = true, DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Jogo { Id = 4, Nome = "World Conqueror", Descricao = "Conquiste nações e domine o mundo", Preco = 39.90m, CategoriaId = 4, Ativo = true, Visivel = true, DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Jogo { Id = 5, Nome = "Soccer Pro 2026", Descricao = "O mais completo simulador de futebol", Preco = 44.90m, CategoriaId = 5, Ativo = true, Visivel = true, DataCadastro = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
