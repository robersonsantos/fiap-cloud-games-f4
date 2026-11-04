namespace FCG.Catalog.Domain.Entities;

public class Jogo
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;
    public bool Ativo { get; set; } = true;
    public bool Visivel { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
    public ICollection<JogoAdquirido> JogosAdquiridos { get; set; } = new List<JogoAdquirido>();

    public void Desativar() => Ativo = false;

    public void Atualizar(string nome, string descricao, decimal preco, int categoriaId, bool visivel)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do jogo não pode ser vazio.", nameof(nome));
        if (preco < 0)
            throw new ArgumentException("Preço do jogo não pode ser negativo.", nameof(preco));

        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        CategoriaId = categoriaId;
        Visivel = visivel;
    }

    public bool EstaDisponivel() => Ativo && Visivel;
}
