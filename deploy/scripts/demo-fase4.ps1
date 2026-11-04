<#
.SYNOPSIS
  Demonstração da Fase 4 contra o gateway (Kong) — local ou no EKS.

.EXAMPLE
  # Local (docker compose)
  ./demo-fase4.ps1 -BaseUrl http://localhost:8000 -AdminEmail admin@fcg.com -AdminSenha '<senha do .env>'

  # AWS (EXTERNAL-IP do Service kong)
  ./demo-fase4.ps1 -BaseUrl http://<elb>.us-east-2.elb.amazonaws.com -AdminEmail admin@fcg.com -AdminSenha '<senha do Secrets Manager>'
#>
param(
    [Parameter(Mandatory)] [string] $BaseUrl,
    [Parameter(Mandatory)] [string] $AdminEmail,
    [Parameter(Mandatory)] [string] $AdminSenha
)

$ErrorActionPreference = 'Stop'
$BaseUrl = $BaseUrl.TrimEnd('/')
$falhas = 0

function Titulo($t) { Write-Host "`n=== $t ===" -ForegroundColor Cyan }
function Ok($t) { Write-Host "  [OK] $t" -ForegroundColor Green }
function Falha($t) { Write-Host "  [FALHA] $t" -ForegroundColor Red; $script:falhas++ }

function Chamar {
    param([string] $Metodo = 'GET', [string] $Caminho, $Corpo, [hashtable] $Headers = @{})
    $params = @{ Method = $Metodo; Uri = "$BaseUrl$Caminho"; Headers = $Headers; UseBasicParsing = $true }
    if ($null -ne $Corpo) {
        $params.ContentType = 'application/json; charset=utf-8'
        $params.Body = [Text.Encoding]::UTF8.GetBytes(($Corpo | ConvertTo-Json -Depth 5))
    }
    $sw = [Diagnostics.Stopwatch]::StartNew()
    try {
        $r = Invoke-WebRequest @params
        $status = [int]$r.StatusCode
        $conteudo = $r.Content
        $tempoApi = $r.Headers['X-Response-Time-Ms']
    } catch [System.Net.WebException] {
        $resp = $_.Exception.Response
        if ($null -eq $resp) { throw }
        $status = [int]$resp.StatusCode
        $conteudo = (New-Object IO.StreamReader($resp.GetResponseStream())).ReadToEnd()
        $tempoApi = $resp.Headers['X-Response-Time-Ms']
    }
    $sw.Stop()
    $json = $null
    if ($conteudo) { try { $json = $conteudo | ConvertFrom-Json } catch { } }
    [pscustomobject]@{ Status = $status; Json = $json; TotalMs = $sw.ElapsedMilliseconds; ApiMs = $tempoApi }
}

Titulo "1. Gateway (Kong) — $BaseUrl"
$r = Chamar -Caminho '/api/jogos'
if ($r.Status -eq 200) { Ok "GET /api/jogos → $($r.Json.Count) jogo(s)" } else { Falha "GET /api/jogos → $($r.Status)" }

$r = Chamar -Metodo POST -Caminho '/api/jogos/1/comprar'
if ($r.Status -eq 401) { Ok 'Compra sem token bloqueada pelo Kong (401)' } else { Falha "Compra sem token → $($r.Status)" }

Titulo '2. Autenticação (administrador inicial vindo do Secret)'
$r = Chamar -Metodo POST -Caminho '/api/autenticacao' -Corpo @{ email = $AdminEmail; senha = $AdminSenha }
if ($r.Status -ne 200) { Falha "Login do admin → $($r.Status)"; exit 1 }
$auth = @{ Authorization = "Bearer $($r.Json.token)" }
Ok "Login OK (role: $($r.Json.role))"

Titulo '3. Busca avançada (Amazon OpenSearch) — fuzzy + relevância'
foreach ($termo in 'dragon', 'dragn', 'kingdon', 'socer', 'acao') {
    $r = Chamar -Caminho "/search?q=$termo"
    if ($r.Status -eq 200 -and $r.Json.total -gt 0) {
        $top = $r.Json.resultados | Select-Object -First 3 | ForEach-Object { "$($_.nome) ($($_.relevancia))" }
        Ok ("'{0}' → {1} [{2} ms no OpenSearch]" -f $termo, ($top -join ', '), $r.Json.tempoMs)
    } else {
        Falha "Busca '$termo' → status $($r.Status), total $($r.Json.total)"
    }
}

Titulo '4. Sincronização SQL Server → OpenSearch (insert/update)'
$nome = "Galactic Racers $(Get-Random -Maximum 9999)"
$r = Chamar -Metodo POST -Caminho '/api/jogos' -Headers $auth -Corpo @{ nome = $nome; descricao = 'Corrida espacial em alta velocidade'; preco = 69.9; categoriaId = 5 }
if ($r.Status -ne 201) { Falha "Criar jogo → $($r.Status)" } else {
    $id = $r.Json.id
    Ok "Jogo criado no SQL Server: $id - $nome"

    $r = Chamar -Caminho '/search?q=galatic'
    if ($r.Json.resultados.id -contains $id) { Ok "Encontrado no OpenSearch com erro de digitação ('galatic')" } else { Falha 'Jogo novo não apareceu na busca' }

    $r = Chamar -Metodo PUT -Caminho "/api/jogos/$id" -Headers $auth -Corpo @{ nome = "Nebula Drift $id"; descricao = 'Corrida espacial'; preco = 59.9; categoriaId = 5; visivel = $true }
    $r = Chamar -Caminho '/search?q=nebulla'
    if ($r.Json.resultados.id -contains $id) { Ok "Edição refletida no índice ('nebulla' → Nebula Drift $id)" } else { Falha 'Edição não refletida no índice' }

    $r = Chamar -Metodo DELETE -Caminho "/api/jogos/$id" -Headers $auth
    $r = Chamar -Caminho '/search?q=nebula'
    if (-not ($r.Json.resultados.id -contains $id)) { Ok 'Remoção refletida no índice' } else { Falha 'Jogo removido continua na busca' }
}

Titulo '5. Cache distribuído (Redis) — MISS x HIT'
# Uma escrita no catálogo invalida o cache; a 1ª leitura vai ao SQL Server, as seguintes ao Redis.
$miss = Chamar -Caminho '/api/jogos'
$hits = 1..5 | ForEach-Object { Chamar -Caminho '/api/jogos' }
$mediaHit = ($hits | ForEach-Object { [double]$_.ApiMs } | Measure-Object -Average).Average
Ok ("1ª chamada (MISS → SQL Server): {0} ms na API" -f $miss.ApiMs)
Ok ("Chamadas seguintes (HIT → Redis): média {0:N1} ms na API" -f $mediaHit)
if ([double]$miss.ApiMs -gt $mediaHit) { Ok ("Ganho do cache: {0:N1}x mais rápido" -f ([double]$miss.ApiMs / [math]::Max($mediaHit, 0.1))) }

Titulo '6. NoSQL (MongoDB) — avaliações'
$r = Chamar -Metodo POST -Caminho '/api/jogos/3/avaliacoes' -Headers $auth -Corpo @{ nota = 5; comentario = 'Demo Fase 4' }
if ($r.Status -in 200, 201) { Ok 'Avaliação gravada no MongoDB' } else { Falha "Avaliação → $($r.Status)" }
$r = Chamar -Caminho '/api/jogos/3/avaliacoes'
if ($r.Status -eq 200) { Ok "Média: $($r.Json.mediaNotas) ($($r.Json.totalAvaliacoes) avaliação(ões))" } else { Falha "Listar avaliações → $($r.Status)" }

Write-Host ''
if ($falhas -eq 0) { Write-Host 'Demonstração concluída sem falhas.' -ForegroundColor Green } else { Write-Host "$falhas verificação(ões) falharam." -ForegroundColor Red; exit 1 }
