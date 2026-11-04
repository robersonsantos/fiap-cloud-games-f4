# FIAP Cloud Games — Fase 4 (AWS)

Plataforma de jogos em microsserviços .NET 9 rodando em **Kubernetes gerenciado na AWS (Amazon EKS)**,
com **CI/CD no GitHub Actions**, **persistência poliglota** (SQL Server + MongoDB), **cache distribuído
(Redis)** e **busca avançada (Amazon OpenSearch)**, sem nenhuma credencial no código.

| Documento | Conteúdo |
|---|---|
| [docs/GUIA-AWS.md](docs/GUIA-AWS.md) | Passo a passo: conta AWS → Terraform → GitHub → deploy → destruir |
| [docs/GUIA-LOCAL.md](docs/GUIA-LOCAL.md) | Executar tudo localmente com Docker Compose |
| [docs/ARQUITETURA.md](docs/ARQUITETURA.md) | Decisões técnicas e mapeamento de cada requisito do enunciado |
| [docs/ROTEIRO-VIDEO-FASE4.md](docs/ROTEIRO-VIDEO-FASE4.md) | Roteiro do "Pitch Final" (até 25 min) com o Live Deploy |
| [docs/RELATORIO-ENTREGA.md](docs/RELATORIO-ENTREGA.md) | Modelo do relatório de entrega |

## Arquitetura

```
                        Internet
                           │
                 ┌─────────▼──────────┐
                 │ AWS Load Balancer  │  (Service type=LoadBalancer)
                 └─────────┬──────────┘
 Amazon EKS ┌──────────────▼───────────────────────────────────────────────┐
 ns "fcg"   │                Kong API Gateway (JWT)                        │
            │        ┌───────────┴───────────┐                             │
            │   ┌────▼─────┐          ┌──────▼──────┐                      │
            │   │ UsersAPI │          │ CatalogAPI  │──── Redis (cache)    │
            │   └──┬───┬───┘          └─┬──┬──┬──┬──┘──── MongoDB (avaliações)
            │      │   │                │  │  │  │                         │
            │      │   └── RabbitMQ ◄───┘  │  │  └──── SQL Server (EBS)   │
            │      │         ▲   ▲         │  │                            │
            │      │   PaymentsAPI  NotificationsAPI                       │
            │      └──────── SQL Server ◄──┘  │                            │
            │                Prometheus + Grafana                          │
            └───────────────────────────────────┼──────────────────────────┘
                                                │ HTTPS
                                  ┌─────────────▼──────────────┐
                                  │ Amazon OpenSearch Service  │ (busca fuzzy)
                                  └────────────────────────────┘
 Amazon ECR (imagens privadas) · AWS Secrets Manager (credenciais) · GitHub Actions · região us-east-2 (Ohio)
```

| Componente | Papel | Tecnologia |
|---|---|---|
| **Kong** | Entrada única, roteamento, validação JWT | Kong 3.9 DB-less |
| **UsersAPI** | Cadastro, login (JWT), administradores | .NET 9 + EF Core + SQL Server |
| **CatalogAPI** | Jogos, biblioteca, compra, avaliações, busca | SQL Server + **MongoDB** + **Redis** + **OpenSearch** |
| **PaymentsAPI** | Processa pedidos (simulado) | Worker .NET + MassTransit |
| **NotificationsAPI** | Notificações de cadastro e compra | Worker .NET + MassTransit |
| **Prometheus/Grafana** | Métricas de latência, throughput e erros | prometheus-net |

## Requisitos da Fase 4 → onde estão

| Requisito | Implementação |
|---|---|
| Kubernetes gerenciado | Amazon EKS 1.36 — [infra/terraform/eks.tf](infra/terraform/eks.tf) |
| Registry privado | Amazon ECR, tags imutáveis + scan on push — [ecr.tf](infra/terraform/ecr.tf) |
| Exposição externa | `Service` LoadBalancer do Kong — [OrchestrationAPI/k8s/kong/service.yaml](OrchestrationAPI/k8s/kong/service.yaml) |
| NoSQL (driver oficial) | MongoDB com `MongoDB.Driver` (avaliações) — [AvaliacaoMongoRepository.cs](CatalogAPI/src/FCG.Catalog.Infrastructure/Repositories/AvaliacaoMongoRepository.cs) |
| Cache distribuído | Redis via `IDistributedCache` (StackExchange.Redis) — [JogoService.cs](CatalogAPI/src/FCG.Catalog.Application/Services/JogoService.cs) |
| Busca com OpenSearch | `OpenSearch.Client` — [OpenSearchJogoIndex.cs](CatalogAPI/src/FCG.Catalog.Infrastructure/Search/OpenSearchJogoIndex.cs) |
| Sincronização do índice | Insert/update/delete de jogo atualizam o índice — `JogoService` |
| `/search` fuzzy + relevância | `multi_match` com `fuzziness: AUTO`, ordenado por `_score` — [BuscaController.cs](CatalogAPI/src/FCG.Catalog.API/Controllers/BuscaController.cs) |
| CI/CD (Users + Catalog, e também Payments/Notifications) | [.github/workflows](.github/workflows) |
| Build & Test | `dotnet build` + `dotnet test` (NUnit) |
| Imagem versionada | Tag `<sha7>-<run>` no ECR |
| Security scan | Amazon ECR image scanning, resultado no resumo do job |
| Deploy sem downtime | `RollingUpdate` (`maxUnavailable: 0`), probes, rollback automático |
| Zero hardcoded credentials | Senhas geradas pelo Terraform → AWS Secrets Manager → Secret `fcg-secrets` |

## Busca avançada

```http
GET /search?q=dragn&tamanho=10
```

```json
{
  "termo": "dragn",
  "total": 1,
  "tempoMs": 12,
  "resultados": [
    { "id": 3, "nome": "Dragon Quest Online", "categoria": "RPG", "preco": 79.90, "relevancia": 4.1203 }
  ]
}
```

- **Fuzzy**: `fuzziness: AUTO` tolera 1–2 letras erradas (`dragn`, `kingdon`, `socer`).
- **Relevância**: nome com peso 3, categoria 2, descrição 1; resultados ordenados por `_score`.
- **Acentos**: analisador com `asciifolding` (`acao` encontra "Ação").
- **Independente do SQL**: a consulta vai somente ao OpenSearch.
- **Sincronização**: `JogoService` indexa em `POST`/`PUT` e remove no `DELETE` de `/api/jogos`. Na subida, o
  pod popula o índice se ele estiver vazio; `POST /api/jogos/search/reindexar` (admin) reconstrói tudo.

## Estrutura do repositório (monorepo)

```
.github/workflows/      Pipelines (um por serviço + plataforma + workflow reutilizável)
infra/terraform/        VPC, EKS, ECR, OpenSearch, Secrets Manager, usuário de deploy do GitHub, Budget
deploy/scripts/         sync-secrets.sh (Secrets Manager → K8s) e demo-fase4.ps1
OrchestrationAPI/       docker-compose local, Kong, monitoramento e manifestos K8s da plataforma
UsersAPI/ CatalogAPI/   Código, testes, Dockerfile e manifestos K8s de cada serviço
PaymentsAPI/ NotificationsAPI/
docs/                   Guias, arquitetura, roteiro do vídeo e relatório
```

## Testes

```powershell
dotnet test UsersAPI/UsersAPI.sln
dotnet test CatalogAPI/CatalogAPI.slnx
dotnet test PaymentsAPI/PaymentsAPI.slnx
dotnet test NotificationsAPI/NotificationsAPI.slnx
```
