# Arquitetura e decisões — Fase 4

## 1. Infraestrutura gerenciada (AWS)

| Decisão | Motivo |
|---|---|
| **Região us-east-2 (Ohio)** | A organização AWS da conta só libera esta região (SCP); preços iguais aos de us-east-1 |
| **Amazon EKS** (Kubernetes 1.36, suporte padrão) | Kubernetes gerenciado exigido; `upgrade_policy = STANDARD` evita a cobrança de suporte estendido |
| **EKS Pod Identity** para o driver EBS CSI | Credenciais exclusivas por *service account*, sem IRSA/OIDC (bloqueado na organização) e sem depender do metadata service do nó |
| Node group gerenciado, 2× t3.medium (AL2023) | Capacidade para bancos + APIs com 2 réplicas; atualização de nós feita pela AWS |
| **Somente subnets públicas, sem NAT Gateway** | Economiza ~US$ 32/mês. Os nós não aceitam conexões externas (security group do EKS); a única entrada é o Load Balancer |
| **Amazon ECR**, tags imutáveis e *scan on push* | Registry privado nativo; cada imagem é rastreável até o commit; a lifecycle policy guarda as 15 últimas |
| **Service LoadBalancer** no Kong | Exposição externa sem instalar controller extra; a AWS provisiona o ELB nas subnets marcadas `kubernetes.io/role/elb` |
| **Terraform** | Toda a infraestrutura é reproduzível (`apply`/`destroy`) e versionada em `infra/terraform` |
| **AWS Budget** | Alerta por e-mail para evitar surpresas na conta |

## 2. Persistência poliglota

| Dado | Armazenamento | Por quê |
|---|---|---|
| Usuários, jogos, categorias, biblioteca | **SQL Server** (EF Core) | Dados relacionais e transacionais (compra, unicidade de e-mail) |
| Avaliações de jogos | **MongoDB** (`MongoDB.Driver`) | Documento flexível (comentário opcional, metadados), alto volume de escrita, 1 avaliação por usuário/jogo via *upsert* |
| Listagens e detalhes de jogos | **Redis** (`IDistributedCache` / StackExchange.Redis) | Leituras repetidas e caras; TTL de 5 min |
| Índice de busca | **Amazon OpenSearch** (`OpenSearch.Client`) | Busca textual tolerante a erros, independente do SQL |

SQL Server e MongoDB rodam no cluster com **volumes EBS gp3 criptografados** (PVC + StorageClass `fcg-gp3`),
então os dados sobrevivem a reinícios de pod. O OpenSearch é o serviço **gerenciado** da AWS, como pede o enunciado.

### Cache (Redis)

- **Cache-aside** no `JogoService`: tenta o Redis; em *miss*, consulta o SQL Server e grava com TTL de 5 min.
- **Invalidação por versão**: as chaves de listagem contêm `v{n}`; qualquer escrita no catálogo incrementa `n`,
  invalidando todas as listagens de uma vez (o `IDistributedCache` não permite varrer chaves).
- **Degradação**: falha no Redis não derruba a API; ela consulta direto o banco.
- O cabeçalho **`X-Response-Time-Ms`** mostra o ganho (MISS × HIT) para o cliente e no vídeo.

### Busca (OpenSearch)

- Índice `fcg-jogos`, com analisador `lowercase + asciifolding` (busca sem acento encontra texto acentuado).
- Consulta: `bool.must = multi_match(nome^3, categoria^2, descricao; fuzziness AUTO; prefix_length 1)`,
  `should = match_phrase_prefix(nome)` para favorecer prefixos e `filter = disponivel:true`.
  A ordenação é por `_score` (relevância).
- **Sincronização**: o `JogoService` atualiza o índice após cada `INSERT`/`UPDATE` e remove no *soft delete*.
  O SQL Server é a fonte da verdade: se o OpenSearch falhar, a escrita é mantida, o erro é registrado e
  `POST /api/jogos/search/reindexar` reconstrói o índice. Na subida, o `SearchIndexInitializer` cria o
  índice e o popula se estiver vazio.
- Falhas na consulta retornam **503** (`BuscaIndisponivelException`), e não 500.

## 3. CI/CD (GitHub Actions)

```
push na main (pasta do serviço)
   └─ Build & Test ── dotnet restore/build/test (NUnit) + artefato TRX
        └─ Docker → ECR + Scan ── tag <sha7>-<run>, push, scan do ECR (resumo no job)
             └─ Deploy EKS ── sync de secrets → kubectl apply → rollout status (rollback se falhar)
```

- Workflow reutilizável `_service-pipeline.yml`, chamado por `users-api.yml`, `catalog-api.yml`,
  `payments-api.yml` e `notifications-api.yml`. O filtro de `paths` faz cada serviço ser publicado só quando muda.
- **Pull requests** executam apenas Build & Test.
- **Identidade da pipeline**: usuário IAM `fcg-github-actions` com permissão mínima (push no ECR,
  `eks:DescribeCluster` e leitura do segredo `fcg/app`). A chave fica apenas nos **GitHub Secrets**
  (criptografados). O ideal seria OIDC, mas a organização AWS da conta bloqueia
  `iam:CreateOpenIDConnectProvider` por *service control policy*.
- **Rolling update sem downtime**: `maxUnavailable: 0`, `maxSurge: 1`, `startupProbe`/`readinessProbe`
  em `/health`, `preStop` de 5 s e 2 réplicas para Users/Catalog/Kong.
- **Rollback automático**: se o `rollout status` falhar, o job executa `kubectl rollout undo`.
- `platform.yml` aplica a infraestrutura do cluster (bancos, Kong, monitoramento) quando ela muda.

## 4. Segurança e gestão de segredos

```
Terraform (random_password) → AWS Secrets Manager "fcg/app"
   → GitHub Actions (usuário fcg-github-actions, GetSecretValue) → Secret Kubernetes "fcg-secrets" (server-side apply)
      → pods (secretKeyRef em tempo de execução)
```

- **Nenhuma** senha, chave JWT ou connection string em código, `appsettings*.json`, YAML ou workflow.
  As APIs falham na inicialização se a configuração não for injetada (sem *fallback* inseguro).
- Connection strings são montadas no pod com interpolação de variáveis do Kubernetes (`$(SA_PASSWORD)`).
- **Kong**: o campo `jwt_secrets.secret` não aceita referência de vault, então `kong.yml` é um *template*
  (`__JWT_KEY__`) renderizado por um `initContainer` num volume em memória. A Admin API fica desligada no cluster.
- MongoDB e Redis passam a exigir autenticação; o RabbitMQ não usa mais `guest`.
- Containers .NET rodam como usuário **não-root** (`USER $APP_UID`).
- Ambiente local: `.env` (fora do Git) com `${VAR:?}` no compose, que falha se faltar alguma variável.
- O `terraform.tfstate` contém as senhas geradas e está no `.gitignore`.

## 5. O que mudou em relação à Fase 3

| Fase 3 | Fase 4 |
|---|---|
| Docker Desktop / K8s local | **Amazon EKS** + ECR + ELB |
| Deploy manual (`docker build` + `kubectl apply`) | **GitHub Actions** com rolling update e rollback |
| Busca `LIKE` no SQL Server | **Amazon OpenSearch** com fuzzy e relevância |
| Senhas em YAML/appsettings | **Secrets Manager → Kubernetes Secret** |
| Notificações em Azure Functions | Worker `NotificationsAPI` no próprio cluster (tudo em uma nuvem) |
| Mongo/Redis sem senha e sem volume | Autenticados; Mongo e SQL com volume EBS |
