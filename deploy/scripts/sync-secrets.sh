#!/usr/bin/env bash
# Sincroniza o segredo do AWS Secrets Manager com o Secret "fcg-secrets" do Kubernetes.
# Usa server-side apply para que os valores não fiquem copiados na anotação
# kubectl.kubernetes.io/last-applied-configuration.
set -euo pipefail

SECRET_ID="${APP_SECRET_ID:-fcg/app}"
NAMESPACE="${K8S_NAMESPACE:-fcg}"

aws secretsmanager get-secret-value --secret-id "$SECRET_ID" --query SecretString --output text \
  | jq --arg ns "$NAMESPACE" '{apiVersion: "v1", kind: "Secret", type: "Opaque",
                              metadata: {name: "fcg-secrets", namespace: $ns,
                                         labels: {"app.kubernetes.io/managed-by": "github-actions"}},
                              stringData: .}' \
  | kubectl apply --server-side --force-conflicts --field-manager=fcg-pipeline -f - >/dev/null

echo "Secret fcg-secrets sincronizado a partir de $SECRET_ID (valores não exibidos)."