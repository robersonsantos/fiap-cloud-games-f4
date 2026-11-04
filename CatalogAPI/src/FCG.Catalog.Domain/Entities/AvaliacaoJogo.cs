namespace FCG.Catalog.Domain.Entities;

/// <summary>
/// Avaliação de um jogo feita por um usuário.
/// Persistida no MongoDB (dados flexíveis / alta volumetria) — Fase 3.
/// </summary>
public class AvaliacaoJogo
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int JogoId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string? Comentario { get; set; }
    public DateTime Data { get; set; } = DateTime.UtcNow;

    public bool NotaValida() => Nota >= 1 && Nota <= 5;
}
