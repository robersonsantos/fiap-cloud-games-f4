# Roteiro do Vídeo — Tech Challenge Fase 4 ("Pitch Final")

**Duração máxima:** 25 minutos (vídeo final: **17:04**) · **Participantes:** 4 pessoas · gravação **sem áudio** (narração inserida depois nas marcações)
**Fonte:** *Entregáveis da Fase 4* no `TC NETT - Fase 4.pdf`
**Texto de cada pessoa, cena a cena:** [FALAS-VIDEO-FASE4.md](FALAS-VIDEO-FASE4.md)

## O que o PDF exige no vídeo

| Item do enunciado | Bloco | Quem |
|---|---|---|
| Infraestrutura rodando na nuvem: **painel da AWS** e **pods no Kubernetes** | 1 | Pessoa 1 |
| **Live Deploy**: alterar código, push, pipeline atualizando produção | 2 | Pessoa 2 |
| **Busca avançada** | 3 | Pessoa 3 |
| **Impacto positivo do cache** na resposta da API | 4 | Pessoa 4 |
| Explicar como o **NoSQL** foi integrado | 4 | Pessoa 4 |

| Papel | Nome | Tela |
|---|---|---|
| Pessoa 1 | Glayson Sergio da Silva | Console AWS + terminal `kubectl` |
| Pessoa 2 | Diogo Henrique Figueiredo Alves da Silva | VS Code + GitHub Actions + `kubectl -w` |
| Pessoa 3 | Roberson Rabelo dos Santos | Terminal (busca) + OpenSearch Dashboards |
| Pessoa 4 | Jhonatan Rodrigues Ferreira de Oliveira | Terminal (cache e Mongo) + Grafana |

> Troquem os nomes/ordem se preferirem.

---

## Preparação (antes de gravar, fora do vídeo)

1. Ambiente no ar conforme [GUIA-AWS.md](GUIA-AWS.md) (seções 4 a 7) e `demo-fase4.ps1` sem falhas.
2. Variáveis no terminal:

   ```powershell
   $GW = "http://$(kubectl get svc kong -n fcg -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')"
   $s  = aws secretsmanager get-secret-value --secret-id fcg/app --query SecretString --output text | ConvertFrom-Json
   $login = Invoke-RestMethod -Method Post "$GW/api/autenticacao" -ContentType 'application/json' `
              -Body (@{ email = $s.ADMIN_EMAIL; senha = $s.ADMIN_PASSWORD } | ConvertTo-Json)
   $auth = @{ Authorization = "Bearer $($login.token)" }
   ```

3. Abas abertas: Console AWS (EKS, ECR, OpenSearch, Secrets Manager, EC2 → Load Balancers),
   GitHub Actions, Grafana (`kubectl port-forward -n fcg svc/grafana 3000:3000`).
4. **Não mostre valores de segredos na tela** (não exiba `$s` inteiro).

---

## Bloco 0 — Abertura (Pessoa 1 · C01 · 0:05–1:25)

> "Somos o Grupo 90. Na Fase 4 levamos a FIAP Cloud Games para a AWS: Kubernetes gerenciado no EKS,
> deploy automático com GitHub Actions, persistência poliglota com SQL Server e MongoDB, cache com Redis
> e busca avançada com Amazon OpenSearch, sem nenhuma senha no código."

Mostrar o diagrama do [README](../README.md).

## Bloco 1 — Infraestrutura na AWS (Pessoa 1 · C02–C06 · 1:26–5:45)

**Console AWS** (região **Ohio / us-east-2**):

1. **EKS → Clusters → `fcg`**: versão 1.36, aba *Compute* (node group com 2 nós), aba *Add-ons*.
2. **ECR → Repositories**: 4 repositórios privados; abrir `fcg/catalog-api` → tags `sha-run` e *scan* de vulnerabilidades.
3. **EC2 → Load Balancers**: o ELB criado pelo Service do Kong.
4. **Amazon OpenSearch → `fcg-search`**.
5. **Secrets Manager → `fcg/app`**: mostrar **apenas as chaves**, sem revelar os valores.
   > "Todas as senhas foram geradas pelo Terraform; ninguém digitou senha nenhuma."

**Terminal:**

```powershell
kubectl get nodes -o wide
kubectl get pods -n fcg -o wide
kubectl get svc kong -n fcg
kubectl get pvc -n fcg                     # volumes EBS do SQL Server e do Mongo
```

Rapidamente: `infra/terraform/` no VS Code (*"toda a infraestrutura é código"*).

**Gateway funcionando pela internet:**

```powershell
Invoke-RestMethod "$GW/api/jogos" | Format-Table id, nome, preco
Invoke-WebRequest -Method Post "$GW/api/jogos/1/comprar" -UseBasicParsing   # 401: Kong bloqueia sem JWT
```

## Bloco 2 — Live Deploy (Pessoa 2 · C07–C10 · 5:46–9:09; resultado em C19 · 15:08–16:08)

1. Mostrar `.github/workflows/_service-pipeline.yml`: Build & Test → Docker/ECR + Scan → Deploy EKS.
   Destacar: **usuário de deploy com permissão mínima** (chave só nos GitHub Secrets), **tag imutável por commit**, **rolling update** e **rollback automático**.
2. Mostrar no `CatalogAPI/k8s/deployment.yaml`: `maxUnavailable: 0`, probes, `secretKeyRef`.
3. Preparar dois terminais lado a lado:

   ```powershell
   # Terminal A — pods sendo trocados
   kubectl get pods -n fcg -l app=catalog-api -w

   # Terminal B — tráfego contínuo (prova de zero downtime)
   while ($true) { $r = Invoke-WebRequest "$GW/api/jogos" -UseBasicParsing; "$(Get-Date -f HH:mm:ss) $($r.StatusCode)"; Start-Sleep -Milliseconds 500 }
   ```

4. **Alteração ao vivo**: em `CatalogAPI/src/FCG.Catalog.API/Controllers/VersaoController.cs`, troque a
   constante `Mensagem` (ex.: `"Live Deploy do Grupo 94 funcionando!"`). Antes, mostre a versão atual:

   ```powershell
   Invoke-RestMethod "$GW/api/catalogo/versao"     # mensagem, imagem (tag do commit) e pod
   ```

   No Terminal B, troque o loop para consultar a versão, assim a troca aparece ao vivo:

   ```powershell
   while ($true) { $v = Invoke-RestMethod "$GW/api/catalogo/versao"; "$(Get-Date -f HH:mm:ss) $($v.pod) $($v.mensagem)"; Start-Sleep -Milliseconds 500 }
   ```

   ```powershell
   git commit -am "feat(catalog): live deploy no Pitch Final"
   git push
   ```

5. **GitHub → Actions → catalog-api**: acompanhar os 3 jobs (testes passando, tag gerada, resumo do scan).
6. No Terminal A, pods novos `Running` antes dos antigos `Terminating`; no Terminal B, a mensagem muda
   para a nova versão **sem nenhum erro** no meio.
7. Fechamento:

   ```powershell
   kubectl rollout history deployment/catalog-api -n fcg
   kubectl get deploy catalog-api -n fcg -o jsonpath='{.spec.template.spec.containers[0].image}'
   ```

> ⏱️ A pipeline leva ~5–7 min. Enquanto roda, siga para os Blocos 3 e 4 e volte para mostrar o resultado.

## Bloco 3 — Busca avançada com OpenSearch (Pessoa 3 · C11–C14 · 9:10–12:08)

> "O endpoint `/search` não consulta o SQL Server: ele vai direto ao Amazon OpenSearch, com fuzzy search
> e ordenação por relevância."

```powershell
# Erros de digitação
Invoke-RestMethod "$GW/search?q=dragn"   | Select-Object -Expand resultados | Format-Table nome, categoria, relevancia
Invoke-RestMethod "$GW/search?q=kingdon" | Select-Object -Expand resultados | Format-Table nome, relevancia
Invoke-RestMethod "$GW/search?q=acao"    | Select-Object -Expand resultados | Format-Table nome, categoria, relevancia   # sem acento

# Sincronização: jogo novo no SQL Server → índice atualizado
$novo = Invoke-RestMethod -Method Post "$GW/api/jogos" -Headers $auth -ContentType 'application/json' `
          -Body '{"nome":"Galactic Racers","descricao":"Corrida espacial","preco":69.9,"categoriaId":5}'
Invoke-RestMethod "$GW/search?q=galatic" | Select-Object -Expand resultados | Format-Table id, nome, relevancia
```

Explicar rapidamente o código: `OpenSearchJogoIndex.BuscarAsync` (`multi_match`, `fuzziness AUTO`, pesos
nome^3/categoria^2, ordenação por `_score`) e `JogoService.SincronizarIndiceAsync`.

Opcional: OpenSearch Dashboards → *Discover* no índice `fcg-jogos`.

## Bloco 4 — Cache (Redis) e NoSQL (MongoDB) (Pessoa 4 · C15–C18 · 12:09–15:07)

**Cache — impacto na resposta:**

```powershell
# A criação do jogo no Bloco 3 invalidou o cache → 1ª chamada é MISS
1..6 | ForEach-Object {
  $r = Invoke-WebRequest "$GW/api/jogos" -UseBasicParsing
  "Chamada $_ → $($r.Headers['X-Response-Time-Ms']) ms na API"
}
kubectl logs -n fcg deploy/catalog-api --tail 30 | Select-String "CACHE"
```

> "A primeira chamada foi ao SQL Server; as seguintes vieram do Redis, com tempo de resposta várias vezes
> menor. Ao alterar o catálogo, a versão do cache muda e tudo é invalidado de uma vez."

Mostrar no Grafana o painel de **latência** caindo após as chamadas em cache.

**NoSQL — como foi integrado:**

```powershell
Invoke-RestMethod -Method Post "$GW/api/jogos/3/avaliacoes" -Headers $auth -ContentType 'application/json' `
  -Body '{"nota":5,"comentario":"Melhor RPG da plataforma"}'
Invoke-RestMethod "$GW/api/jogos/3/avaliacoes"
kubectl exec -n fcg deploy/mongodb -- sh -c 'mongosh -u "$MONGO_INITDB_ROOT_USERNAME" -p "$MONGO_INITDB_ROOT_PASSWORD" --quiet FCG_Catalog --eval "db.avaliacoes.find().limit(3)"'
```

> "Avaliações são dados flexíveis e de alto volume, então ficam no MongoDB com o driver oficial
> `MongoDB.Driver`. Jogos e compras continuam no SQL Server, que é transacional. Cada tipo de dado
> fica no banco mais adequado: isso é persistência poliglota."

## Bloco 5 — Encerramento (Pessoa 1 · C20 · 16:09–16:59)

- Voltar ao GitHub Actions: pipeline do Live Deploy **verde**.
- Recapitular: EKS + ECR + ELB, CI/CD com rolling update, OpenSearch, Redis, MongoDB e Secrets Manager.
- "Para controlar custos, o ambiente é destruído com `terraform destroy` após a gravação."

---

## Checklist pós-gravação

- [ ] Vídeo no YouTube (não listado) ou Drive com acesso liberado
- [ ] Links no [RELATORIO-ENTREGA.md](RELATORIO-ENTREGA.md)
- [ ] Ambiente destruído ([GUIA-AWS.md](GUIA-AWS.md), seção 9)
