# FCG Catalog API — Fase 4

Catálogo de jogos, biblioteca, compra, avaliações e busca da **FIAP Cloud Games**, com persistência poliglota:

| Dado | Tecnologia |
|---|---|
| Jogos, categorias, biblioteca | SQL Server (EF Core) |
| Avaliações | **MongoDB** (`MongoDB.Driver`) |
| Cache de listagens/detalhes | **Redis** (`IDistributedCache`) |
| Busca textual | **Amazon OpenSearch** (`OpenSearch.Client`) |

Documentação geral: [README](../README.md) · Decisões: [docs/ARQUITETURA.md](../docs/ARQUITETURA.md)

## Endpoints (via Kong)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| GET | `/search?q={termo}&tamanho={n}` | Público | **Busca fuzzy ordenada por relevância (OpenSearch)** |
| GET | `/api/jogos/search?q=` | Público | Mesmo endpoint, sob o prefixo do catálogo |
| POST | `/api/jogos/search/reindexar` | Administrador | Reconstrói o índice a partir do SQL Server |
| GET | `/api/jogos` | Público | Lista (cache Redis; cabeçalho `X-Response-Time-Ms`) |
| GET | `/api/jogos/{id}` | Público | Detalhe (cache Redis) |
| POST/PUT/DELETE | `/api/jogos[/{id}]` | Administrador | Escrita: invalida o cache e **sincroniza o índice** |
| POST | `/api/jogos/{id}/comprar` | Autenticado | Publica `OrderPlacedEvent` |
| GET/POST/DELETE | `/api/jogos/{id}/avaliacoes` | GET público | Avaliações no MongoDB |
| GET | `/api/biblioteca` | Autenticado | Jogos adquiridos |
| GET/POST/PUT/DELETE | `/api/categorias` | Escrita: Administrador | Categorias |
| GET | `/api/catalogo/versao` | Público | Mensagem, imagem implantada e pod (demo do Live Deploy) |

## Busca (OpenSearch)

- `Infrastructure/Search/OpenSearchJogoIndex.cs`: índice `fcg-jogos`, analisador com `asciifolding`,
  `multi_match` com `fuzziness: AUTO` (nome^3, categoria^2, descrição) e ordenação por `_score`.
- `Application/Services/JogoService.cs`: indexa após criar/editar e remove após desativar.
- `API/HostedServices/SearchIndexInitializer.cs`: na subida, cria o índice e o popula se estiver vazio.

## Configuração

| Variável | Origem |
|---|---|
| `ConnectionStrings__DefaultConnection` | Montada no pod com `SA_PASSWORD` |
| `MongoDb__ConnectionString` | Montada com `MONGO_USER`/`MONGO_PASSWORD` |
| `Redis__ConnectionString` | Montada com `REDIS_PASSWORD` |
| `OpenSearch__Url`, `OpenSearch__Username`, `OpenSearch__Password` | Secret (endpoint do domínio gerenciado) |
| `Jwt__Chave`, `RabbitMQ__Username`, `RabbitMQ__Password` | Secret |

## Testes e pipeline

```powershell
dotnet test CatalogAPI.slnx
```

Pipeline: [.github/workflows/catalog-api.yml](../.github/workflows/catalog-api.yml).