# Relatório de Entrega — Tech Challenge Fase 4

## Equipe

- **Nome do grupo:** Grupo 90
- **Participantes e usernames no Discord:**
  - Diogo Henrique Figueiredo Alves da Silva — `diogo_henrique` — RM 372819
  - Roberson Rabelo dos Santos — `Roberson Rabelo` — RM 371058
  - Jhonatan Rodrigues Ferreira de Oliveira — `Jhonatan Rodrigues` — RM 370366
  - Glayson Sergio da Silva — `Glayson Sergio` — RM 370540

## Links

- **Documentação (README detalhado):** https://github.com/<usuario>/fiap-cloud-games-f4#readme
- **Repositório:** https://github.com/<usuario>/fiap-cloud-games-f4
- **Vídeo "Pitch Final":** <link do YouTube ou Drive>

## Resumo da solução

| Requisito | Entrega |
|---|---|
| Cloud e Kubernetes gerenciado | AWS — Amazon EKS 1.36 (`us-east-2`, Ohio) |
| Registry privado | Amazon ECR (4 repositórios, tags imutáveis, scan on push) |
| Exposição externa | Kong API Gateway via Service LoadBalancer (AWS ELB) |
| NoSQL | MongoDB (`MongoDB.Driver`): avaliações de jogos |
| Cache distribuído | Redis (`IDistributedCache` / StackExchange.Redis): listagens e detalhes de jogos |
| Busca avançada | Amazon OpenSearch (`OpenSearch.Client`): `/search` com fuzzy e relevância, sincronizado no CatalogAPI |
| CI/CD | GitHub Actions: build, testes, imagem versionada, scan, push no ECR e rolling update no EKS (Users, Catalog, Payments, Notifications) |
| Segredos | Terraform → AWS Secrets Manager → Secret do Kubernetes; nenhuma credencial no código ou em YAML |
| Infraestrutura como código | Terraform (`infra/terraform`) + manifestos Kubernetes |

## Onde encontrar no repositório

- Pipelines: `.github/workflows/`
- Infraestrutura: `infra/terraform/`
- Manifestos Kubernetes: `OrchestrationAPI/k8s/` e `<Serviço>/k8s/`
- Busca: `CatalogAPI/src/FCG.Catalog.Infrastructure/Search/`
- Guias: `docs/GUIA-AWS.md`, `docs/GUIA-LOCAL.md`, `docs/ARQUITETURA.md`
