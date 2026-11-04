namespace FCG.Catalog.Infrastructure.Search;

public class OpenSearchSettings
{
    public const string SectionName = "OpenSearch";

    /// <summary>Endpoint do domínio (ex.: https://search-fcg-xxxx.us-east-2.es.amazonaws.com).</summary>
    public string Url { get; set; } = "http://localhost:9200";

    /// <summary>Usuário do fine-grained access control. Vazio = sem autenticação (ambiente local).</summary>
    public string? Username { get; set; }

    public string? Password { get; set; }

    public string IndexName { get; set; } = "fcg-jogos";
}
