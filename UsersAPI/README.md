# FCG Users API — Fase 4

Cadastro, autenticação JWT e administradores da **FIAP Cloud Games**. Publica `UserCreatedEvent` no RabbitMQ
(MassTransit) e expõe métricas Prometheus em `GET /metrics`.

Documentação geral: [README](../README.md) · Deploy: [docs/GUIA-AWS.md](../docs/GUIA-AWS.md) ·
Execução local: [docs/GUIA-LOCAL.md](../docs/GUIA-LOCAL.md)

## Arquitetura (Clean Architecture)

```
src/FCG.Users.Domain          Entidades e regras (Usuario, Administrador, SenhaHelper)
src/FCG.Users.Application     DTOs, interfaces e serviços
src/FCG.Users.Infrastructure  EF Core (SQL Server), repositórios, MassTransit
src/FCG.Users.API             Controllers, middlewares, Program.cs
tests/                        NUnit + Moq
k8s/                          Deployment (rolling update) e Service
```

## Endpoints (via Kong)

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/api/autenticacao` | Público | Login, retorna JWT |
| POST | `/api/usuarios` | Público | Cadastro |
| GET | `/api/usuarios` | Administrador | Lista usuários |
| GET | `/api/usuarios/{id}` | Autenticado | Detalhe |
| PUT | `/api/usuarios/{id}` | Administrador | Atualiza nome/e-mail |
| DELETE | `/api/usuarios/{id}` | Administrador | Desativa (soft delete) |
| PUT/DELETE | `/api/usuarios/{id}/admin` | Administrador | Concede/revoga administrador |

## Configuração

Credenciais **não** ficam no código: em produção vêm do Secret `fcg-secrets` (AWS Secrets Manager);
localmente, do `.env` do compose. A API não sobe se faltar alguma.

| Variável | Origem |
|---|---|
| `ConnectionStrings__DefaultConnection` | Montada no pod com `SA_PASSWORD` do Secret |
| `Jwt__Chave` | Secret (`JWT_KEY`) |
| `RabbitMQ__Username` / `RabbitMQ__Password` | Secret |
| `Admin__Email` / `Admin__Senha` | Secret: administrador inicial criado/promovido na subida (idempotente) |
| `Jwt__Emissor`, `Jwt__Audiencia`, `Jwt__ExpiracaoHoras`, `RabbitMQ__Host` | ConfigMap `fcg-shared-config` |

## Testes e pipeline

```powershell
dotnet test UsersAPI.sln
```

Pipeline: [.github/workflows/users-api.yml](../.github/workflows/users-api.yml). Um push em `UsersAPI/**` na `main`
executa testes, gera a imagem `fcg/users-api:<sha>-<run>` no ECR e faz rolling update no EKS.