namespace FCG.Catalog.Domain.Entities;

public class JogoAdquirido
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int JogoId { get; set; }
    public Jogo Jogo { get; set; } = null!;
    public decimal ValorPago { get; set; }
    public DateTime DataAquisicao { get; set; } = DateTime.UtcNow;
    public Guid TransacaoId { get; set; }
}
