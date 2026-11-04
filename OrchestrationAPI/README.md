# OrchestrationAPI — plataforma da Fase 4

Tudo o que não é código de microsserviço: gateway, monitoramento, bancos e o ambiente local.

```
docker-compose.yml        Ambiente local completo (usa .env — copie de .env.example)
kong/kong.yml             Rotas e JWT do Kong (template: __JWT_KEY__ é preenchido na subida)
monitoring/               Prometheus e dashboards do Grafana (compose)
k8s/
  namespace.yaml          Namespace fcg
  storageclass.yaml       EBS gp3 criptografado (fcg-gp3)
  shared-configmap.yaml   Configuração não sensível compartilhada
  sqlserver/ mongodb/     Bancos com PVC (EBS)
  redis/ rabbitmq/        Cache e mensageria (com senha)
  kong/                   Gateway (2 réplicas) + Service LoadBalancer (AWS ELB)
  monitoring/             Prometheus e Grafana
```

- O Secret `fcg-secrets` **não é versionado**: a pipeline o cria a partir do AWS Secrets Manager
  ([deploy/scripts/sync-secrets.sh](../deploy/scripts/sync-secrets.sh)).
- No EKS, estes manifestos são aplicados pelo workflow [platform.yml](../.github/workflows/platform.yml).
- Guias: [AWS](../docs/GUIA-AWS.md) · [Local](../docs/GUIA-LOCAL.md)