namespace FCG.Catalog.Application.DTOs;

/// <summary>Documento do jogo como é armazenado no índice do OpenSearch (Fase 4).</summary>
public record JogoDocumentoBusca(
    int Id,
    string Nome,
    string Descricao,
    decimal Preco,
    string Categoria,
    bool Disponivel,
    DateTime DataCadastro);

/// <summary>Item retornado pelo endpoint /search, ordenado por relevância.</summary>
public record JogoBuscaResultadoDto(
    int Id,
    string Nome,
    string Descricao,
    decimal Preco,
    string Categoria,
    double Relevancia);

public record BuscaResponseDto(
    string Termo,
    int Total,
    long TempoMs,
    IReadOnlyList<JogoBuscaResultadoDto> Resultados);

public record ReindexacaoResponseDto(int JogosIndexados);
