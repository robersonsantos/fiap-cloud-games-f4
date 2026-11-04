using FCG.Catalog.Application.DTOs;
using FCG.Catalog.Application.Exceptions;
using FCG.Catalog.Application.Interfaces;
using OpenSearch.Client;

namespace FCG.Catalog.Infrastructure.Search;

public class OpenSearchJogoIndex : IJogoSearchIndex
{
    private const string Analisador = "fcg_pt_folding";

    private readonly IOpenSearchClient _client;
    private readonly string _indice;

    public OpenSearchJogoIndex(IOpenSearchClient client, OpenSearchSettings settings)
    {
        _client = client;
        _indice = settings.IndexName;
    }

    public static IOpenSearchClient CriarCliente(OpenSearchSettings settings)
    {
        var connection = new ConnectionSettings(new Uri(settings.Url))
            .DefaultIndex(settings.IndexName)
            .RequestTimeout(TimeSpan.FromSeconds(10))
            .ThrowExceptions(false);

        if (!string.IsNullOrWhiteSpace(settings.Username))
            connection = connection.BasicAuthentication(settings.Username, settings.Password);

        return new OpenSearchClient(connection);
    }

    public async Task GarantirIndiceAsync(CancellationToken ct = default)
    {
        var existe = await _client.Indices.ExistsAsync(_indice, ct: ct);
        if (existe.Exists)
            return;

        // Analisador com lowercase + asciifolding: "acao" encontra "Ação".
        var resposta = await _client.Indices.CreateAsync(_indice, c => c
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom(Analisador, ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "asciifolding")))))
            .Map<JogoDocumentoBusca>(m => m
                .Properties(p => p
                    .Number(n => n.Name(d => d.Id).Type(NumberType.Integer))
                    .Text(t => t.Name(d => d.Nome).Analyzer(Analisador)
                        .Fields(f => f.Keyword(k => k.Name("raw"))))
                    .Text(t => t.Name(d => d.Descricao).Analyzer(Analisador))
                    .Text(t => t.Name(d => d.Categoria).Analyzer(Analisador)
                        .Fields(f => f.Keyword(k => k.Name("raw"))))
                    .Number(n => n.Name(d => d.Preco).Type(NumberType.ScaledFloat).ScalingFactor(100))
                    .Boolean(b => b.Name(d => d.Disponivel))
                    .Date(d => d.Name(x => x.DataCadastro)))), ct);

        // Outro pod pode ter criado o índice ao mesmo tempo.
        if (!resposta.IsValid && resposta.ServerError?.Error?.Type != "resource_already_exists_exception")
            throw Falha("criar o índice", resposta);
    }

    public async Task<long> ContarAsync(CancellationToken ct = default)
    {
        var resposta = await _client.CountAsync<JogoDocumentoBusca>(c => c.Index(_indice), ct);
        if (!resposta.IsValid)
            throw Falha("contar documentos", resposta);
        return resposta.Count;
    }

    public async Task IndexarAsync(JogoDocumentoBusca documento, CancellationToken ct = default)
    {
        var resposta = await _client.IndexAsync(documento, i => i
            .Index(_indice)
            .Id(documento.Id)
            .Refresh(OpenSearch.Net.Refresh.WaitFor), ct);

        if (!resposta.IsValid)
            throw Falha($"indexar o jogo {documento.Id}", resposta);
    }

    public async Task IndexarLoteAsync(IReadOnlyCollection<JogoDocumentoBusca> documentos, CancellationToken ct = default)
    {
        if (documentos.Count == 0)
            return;

        var resposta = await _client.BulkAsync(b => b
            .Index(_indice)
            .Refresh(OpenSearch.Net.Refresh.WaitFor)
            .IndexMany(documentos, (op, doc) => op.Id(doc.Id)), ct);

        if (!resposta.IsValid || resposta.Errors)
            throw Falha("indexar em lote", resposta);
    }

    public async Task RemoverAsync(int jogoId, CancellationToken ct = default)
    {
        var resposta = await _client.DeleteAsync<JogoDocumentoBusca>(jogoId, d => d
            .Index(_indice)
            .Refresh(OpenSearch.Net.Refresh.WaitFor), ct);

        if (!resposta.IsValid && resposta.Result != Result.NotFound)
            throw Falha($"remover o jogo {jogoId}", resposta);
    }

    public async Task<IReadOnlyList<JogoBuscaResultadoDto>> BuscarAsync(string termo, int tamanho, CancellationToken ct = default)
    {
        // multi_match com fuzziness AUTO tolera erros de digitação ("zelda" ~ "zelad").
        // Nome pesa 3x e categoria 2x; o resultado vem ordenado por _score (relevância).
        var resposta = await _client.SearchAsync<JogoDocumentoBusca>(s => s
            .Index(_indice)
            .Size(tamanho)
            .TrackScores()
            .Query(q => q
                .Bool(b => b
                    .Must(m => m
                        .MultiMatch(mm => mm
                            .Query(termo)
                            .Fields(f => f
                                .Field(d => d.Nome, 3)
                                .Field(d => d.Categoria, 2)
                                .Field(d => d.Descricao))
                            .Fuzziness(Fuzziness.Auto)
                            .PrefixLength(1)
                            .MaxExpansions(50)))
                    .Should(sh => sh
                        .MatchPhrasePrefix(mp => mp
                            .Field(d => d.Nome)
                            .Query(termo)
                            .Boost(2)))
                    .Filter(f => f
                        .Term(t => t.Field(d => d.Disponivel).Value(true)))))
            .Sort(so => so.Descending(SortSpecialField.Score)), ct);

        if (!resposta.IsValid)
            throw Falha("buscar jogos", resposta);

        return resposta.Hits
            .Select(h => new JogoBuscaResultadoDto(
                h.Source.Id, h.Source.Nome, h.Source.Descricao, h.Source.Preco, h.Source.Categoria,
                Math.Round(h.Score ?? 0, 4)))
            .ToList();
    }

    private static BuscaIndisponivelException Falha(string operacao, IResponse resposta) =>
        new($"OpenSearch: falha ao {operacao}. {resposta.ServerError?.Error?.Reason ?? resposta.OriginalException?.Message ?? "sem detalhes"}",
            resposta.OriginalException);
}
