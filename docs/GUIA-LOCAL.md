# Guia local — Docker Compose

Ambiente completo na máquina, equivalente ao do EKS (o OpenSearch roda em container em vez do serviço gerenciado).

## Pré-requisitos

- Docker Desktop com **pelo menos 6 GB de memória** (Settings → Resources)
- PowerShell

## Subir

```powershell
cd OrchestrationAPI
Copy-Item .env.example .env
notepad .env              # troque TODAS as senhas (o .env não é versionado)
docker compose up --build -d
docker compose ps         # aguarde todos "healthy"/"running" (~2 min após o download das imagens)
```

| Serviço | Endereço |
|---|---|
| **Kong (entrada única)** | http://localhost:8000 |
| UsersAPI (direto, Swagger) | http://localhost:5001/swagger |
| CatalogAPI (direto, Swagger) | http://localhost:5002/swagger |
| OpenSearch | http://localhost:9200 |
| OpenSearch Dashboards | http://localhost:5601 |
| RabbitMQ Management | http://localhost:15672 (`RABBITMQ_USER` / `RABBITMQ_PASSWORD`) |
| Prometheus | http://localhost:9090 |
| Grafana | http://localhost:3000 (`admin` / `GRAFANA_PASSWORD`) |

## Demonstração automatizada

```powershell
$cfg = Get-Content .env | ConvertFrom-StringData
../deploy/scripts/demo-fase4.ps1 -BaseUrl http://localhost:8000 -AdminEmail $cfg.ADMIN_EMAIL -AdminSenha $cfg.ADMIN_PASSWORD
```

## Comandos manuais úteis

```powershell
# Busca fuzzy (erro de digitação) ordenada por relevância
Invoke-RestMethod "http://localhost:8000/search?q=dragn"

# Índice no OpenSearch
Invoke-RestMethod "http://localhost:9200/fcg-jogos/_search?pretty"

# Cache: compare o cabeçalho X-Response-Time-Ms (1ª = MISS no SQL Server, 2ª = HIT no Redis)
1..2 | % { (Invoke-WebRequest "http://localhost:8000/api/jogos" -UseBasicParsing).Headers['X-Response-Time-Ms'] }
docker compose logs catalog-api | Select-String "CACHE|SEARCH"
```

## Encerrar

```powershell
docker compose down        # mantém as imagens
docker compose down -v     # também apaga o volume do Kong
```
