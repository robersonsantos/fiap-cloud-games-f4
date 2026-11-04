using FCG.Catalog.Application.DTOs;

namespace FCG.Catalog.Application.Interfaces;

/// <summary>
/// Motor de busca independente do banco relacional (Amazon OpenSearch / Elasticsearch).
/// </summary>
public interface IJogoSearchIndex
{
    Task GarantirIndiceAsync(CancellationToken ct = default);
    Task<long> ContarAsync(CancellationToken ct = default);
    Task IndexarAsync(JogoDocumentoBusca documento, CancellationToken ct = default);
    Task IndexarLoteAsync(IReadOnlyCollection<JogoDocumentoBusca> documentos, CancellationToken ct = default);
    Task RemoverAsync(int jogoId, CancellationToken ct = default);

    /// <summary>Busca fuzzy (tolerante a erros de digitação) ordenada por relevância (_score).</summary>
    Task<IReadOnlyList<JogoBuscaResultadoDto>> BuscarAsync(string termo, int tamanho, CancellationToken ct = default);
}
