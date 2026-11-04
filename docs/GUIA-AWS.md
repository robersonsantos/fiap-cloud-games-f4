# Guia AWS — FIAP Cloud Games Fase 4

Passo a passo completo para colocar a plataforma no **Amazon EKS** com deploy automático pelo
**GitHub Actions**. Todos os comandos são para **PowerShell no Windows** (executar a partir da raiz do repositório).

> ⏱️ Tempo total: ~40 min (o EKS leva ~15 min e o OpenSearch ~20 min para ficarem prontos, em paralelo).

---

## 0. Custos — leia antes

> **Região:** a organização AWS desta conta só libera **us-east-2 (Ohio)** — tudo foi configurado para ela.
> Nas demais regiões as chamadas retornam *explicit deny in a service control policy*.

O EKS **não tem camada gratuita**. Estimativa em `us-east-2` (mesmos preços de us-east-1) com os padrões do Terraform:

| Recurso | US$/hora | US$/dia (24h) |
|---|---|---|
| EKS control plane | 0,10 | 2,40 |
| 2× EC2 t3.medium (nós) | 0,083 | 2,00 |
| OpenSearch t3.small.search + 10 GB | 0,037 | 0,90 |
| Load Balancer (Kong) | 0,025 | 0,60 |
| EBS (2 nós + volumes dos bancos) | ~0,01 | ~0,25 |
| **Total aproximado** | **~0,26** | **~6,20** |

- Os **créditos de boas-vindas (US$ 100 a 200)** cobrem vários dias de uso.
- **Contas no "Free plan"** só acessam alguns serviços. Se o Terraform retornar erro de permissão/assinatura ao criar
  o EKS, o OpenSearch ou o Load Balancer, faça o upgrade em
  **Billing → Upgrade plan** (<https://console.aws.amazon.com/billing/home?#/freetier/upgrade>).
  Os créditos restantes continuam valendo; só o que passar dos créditos é cobrado.
- Estratégia recomendada: **suba, grave o vídeo e destrua** (seção 9). Recriar leva ~40 min.
- O Terraform cria um **AWS Budget** que envia e-mail ao atingir 50%, 80% e 100% do limite (padrão US$ 30).
- Para economizar ~70% nos nós: `node_capacity_type = "SPOT"` no `terraform.tfvars`.

---

## 1. Ferramentas

```powershell
winget install -e --id Amazon.AWSCLI
winget install -e --id Hashicorp.Terraform
winget install -e --id Kubernetes.kubectl
winget install -e --id GitHub.cli
winget install -e --id Git.Git
```

Feche e reabra o terminal e confira: `aws --version; terraform version; kubectl version --client; gh --version`.

---

## 2. Usuário IAM para o seu computador

**Não use o usuário raiz (e-mail da conta) no dia a dia.**

1. Console AWS → **IAM → Users → Create user** → nome `fcg-admin`.
2. **Attach policies directly** → `AdministratorAccess` → Create.
3. Abra o usuário → **Security credentials → Create access key → Command Line Interface (CLI)**.
4. Configure o CLI (a chave fica só no seu PC, em `%USERPROFILE%\.aws\credentials`):

```powershell
aws configure
# AWS Access Key ID:     (cole)
# AWS Secret Access Key: (cole)
# Default region name:   us-east-2
# Default output format: json

aws sts get-caller-identity   # deve mostrar o ARN do usuário fcg-admin
```

---

## 3. Repositório no GitHub

```powershell
gh auth login                       # GitHub.com → HTTPS → login pelo navegador
git init -b main
git add .
git commit -m "FIAP Cloud Games - Fase 4"
gh repo create fiap-cloud-games-f4 --public --source . --push
gh repo view --json nameWithOwner -q .nameWithOwner   # anote: <usuario>/fiap-cloud-games-f4
```

> O primeiro push dispara os workflows, que vão **falhar no deploy** porque a AWS ainda não existe.
> É esperado; rode-os novamente no passo 6.

---

## 4. Infraestrutura com Terraform

```powershell
cd infra/terraform
Copy-Item terraform.tfvars.example terraform.tfvars
notepad terraform.tfvars
```

Ajuste no `terraform.tfvars`:

```hcl
region       = "us-east-2"
budget_email = "seu-email@exemplo.com"
```

```powershell
terraform init
terraform plan -out fcg.tfplan      # revise o que será criado
terraform apply fcg.tfplan          # ~20 min
```

O Terraform cria:

| Recurso | Para quê |
|---|---|
| VPC + 2 subnets públicas | Rede do cluster (sem NAT Gateway, para reduzir custo) |
| EKS 1.36 + node group (2× t3.medium) | Kubernetes gerenciado |
| Add-ons vpc-cni, kube-proxy, coredns, eks-pod-identity-agent, aws-ebs-csi-driver | Rede, DNS e volumes EBS (o driver recebe credenciais próprias via **EKS Pod Identity**) |
| 4 repositórios ECR | Imagens privadas (tags imutáveis + scan on push) |
| Amazon OpenSearch (`fcg-search`) | Busca avançada |
| Secrets Manager `fcg/app` | **Todas** as senhas, geradas automaticamente |
| Usuário IAM `fcg-github-actions` (permissão mínima) | Identidade da pipeline: só ECR, `eks:DescribeCluster` e leitura do segredo `fcg/app` |
| Role `fcg-console-viewer` (somente leitura) | Navegar no Console AWS e ver nós/pods do EKS sem poder alterar nada nem ler valores de segredos |
| AWS Budget | Alerta de custo por e-mail (confirme a inscrição no e-mail recebido) |

> **Por que não OIDC?** A organização AWS desta conta bloqueia `iam:CreateOpenIDConnectProvider`.
> A pipeline usa então um usuário IAM exclusivo, com permissões mínimas; a chave dele fica só nos
> **GitHub Secrets** (criptografados), nunca no código.

> O `terraform.tfstate` contém as senhas geradas e a chave da pipeline. Ele está no `.gitignore` — **não versione**.

---

## 5. Conectar o kubectl e configurar o GitHub

Ainda dentro de `infra/terraform`:

```powershell
# kubectl → EKS
aws eks update-kubeconfig --region us-east-2 --name fcg
kubectl get nodes                    # 2 nós Ready

# Secrets do GitHub (criptografados; a chave vai direto do Terraform para o GitHub, sem aparecer na tela)
terraform output -raw github_actions_access_key_id     | gh secret set AWS_ACCESS_KEY_ID
terraform output -raw github_actions_secret_access_key | gh secret set AWS_SECRET_ACCESS_KEY

# Variáveis do GitHub (não são segredos)
gh variable set AWS_REGION --body "us-east-2"
gh variable set EKS_CLUSTER_NAME --body "fcg"
gh variable set APP_SECRET_ID --body "fcg/app"

cd ../..
```

Crie o **environment** `production` (usado pelos jobs de deploy):
GitHub → repositório → **Settings → Environments → New environment → `production`**.
Opcional: marque **Required reviewers** para aprovar cada deploy manualmente.

---

## 6. Primeiro deploy

1. **Plataforma** (namespace, Secret, bancos, Redis, RabbitMQ, Kong + Load Balancer, Prometheus/Grafana):

   ```powershell
   gh workflow run platform.yml
   gh run watch                       # ~5 min
   ```

2. **Microsserviços** (build → testes → imagem → ECR → scan → rolling update):

   ```powershell
   gh workflow run users-api.yml
   gh workflow run catalog-api.yml
   gh workflow run payments-api.yml
   gh workflow run notifications-api.yml
   gh run list --limit 5
   ```

3. Conferir:

   ```powershell
   kubectl get pods -n fcg
   kubectl get svc kong -n fcg        # EXTERNAL-IP = endereço público
   ```

> O DNS do Load Balancer leva 2–3 min para responder após aparecer.

---

## 7. Testar

```powershell
$GW = "http://$(kubectl get svc kong -n fcg -o jsonpath='{.status.loadBalancer.ingress[0].hostname}')"

# Senha do administrador inicial (gerada pelo Terraform, guardada no Secrets Manager)
$segredo = aws secretsmanager get-secret-value --secret-id fcg/app --query SecretString --output text | ConvertFrom-Json

./deploy/scripts/demo-fase4.ps1 -BaseUrl $GW -AdminEmail $segredo.ADMIN_EMAIL -AdminSenha $segredo.ADMIN_PASSWORD
```

O script valida gateway, JWT, busca fuzzy, sincronização do índice, cache (MISS × HIT) e MongoDB.

Acessos úteis:

```powershell
# Grafana (usuário admin, senha em $segredo.GRAFANA_PASSWORD)
kubectl port-forward -n fcg svc/grafana 3000:3000

# Prometheus
kubectl port-forward -n fcg svc/prometheus 9090:9090

# OpenSearch Dashboards (usuário fcgadmin, senha em $segredo.OPENSEARCH_PASSWORD)
terraform -chdir=infra/terraform output -raw opensearch_dashboards_url
```

---

## 8. Live deploy (CI/CD)

1. Altere a constante `Mensagem` em `CatalogAPI/src/FCG.Catalog.API/Controllers/VersaoController.cs`.
2. `git commit -am "feat: ..."; git push`
3. GitHub → **Actions** → `catalog-api`: Build & Test → Docker → ECR + Scan → Deploy EKS.
4. Em outro terminal, mostre o rolling update sem downtime:

   ```powershell
   kubectl get pods -n fcg -l app=catalog-api -w
   # e, em paralelo, requisições contínuas (a mensagem muda sem nenhum erro):
   while ($true) { $v = Invoke-RestMethod "$GW/api/catalogo/versao"; "$($v.pod) $($v.mensagem)"; Start-Sleep -Milliseconds 300 }
   ```

5. Histórico de versões: `kubectl rollout history deployment/catalog-api -n fcg`.

---

## 9. Destruir tudo (parar a cobrança)

```powershell
# 1º remove o Load Balancer e os volumes criados pelo Kubernetes (senão impedem a exclusão da VPC)
kubectl delete svc kong -n fcg
kubectl delete namespace fcg

# 2º destrói a infraestrutura
cd infra/terraform
terraform destroy
```

Confira no console (região **us-east-2 / Ohio**) que não sobrou nada em **EC2 → Load Balancers**,
**EC2 → Volumes** e **EKS**.

---

## Solução de problemas

| Sintoma | Causa / solução |
|---|---|
| Workflow falha em *Credenciais AWS* | Secrets `AWS_ACCESS_KEY_ID`/`AWS_SECRET_ACCESS_KEY` ausentes — refaça o passo 5 |
| *Plataforma não encontrada no cluster* | Rode `platform.yml` antes dos serviços |
| Pod `Pending` com PVC | Add-on `aws-ebs-csi-driver` ainda instalando; aguarde e `kubectl describe pvc -n fcg` |
| `catalog-api` reiniciando | `kubectl logs -n fcg deploy/catalog-api` — normalmente o SQL Server ainda está subindo (se resolve sozinho) |
| Busca retorna 503 | OpenSearch indisponível; `kubectl logs -n fcg deploy/catalog-api \| Select-String SEARCH` |
| `EXTERNAL-IP` em `<pending>` | Aguarde; confira as tags `kubernetes.io/role/elb` nas subnets |
| `explicit deny in a service control policy` | Região diferente de us-east-2, ou conta ainda no Free plan (seção 0) |
