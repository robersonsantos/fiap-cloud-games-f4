namespace FCG.Catalog.Domain.Entities;

public class Categoria
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public ICollection<Jogo> Jogos { get; set; } = new List<Jogo>();

    public void Desativar() => Ativo = false;

    public void Atualizar(string nome, string descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da categoria não pode ser vazio.", nameof(nome));

        Nome = nome;
        Descricao = descricao;
    }
}
