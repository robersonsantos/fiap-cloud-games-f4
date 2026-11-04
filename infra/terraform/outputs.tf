output "region" {
  value = var.region
}

output "cluster_name" {
  value = aws_eks_cluster.this.name
}

output "kubeconfig_command" {
  description = "Comando para apontar o kubectl para o cluster."
  value       = "aws eks update-kubeconfig --region ${var.region} --name ${aws_eks_cluster.this.name}"
}

output "ecr_registry" {
  value = "${local.account_id}.dkr.ecr.${var.region}.amazonaws.com"
}

output "ecr_repositories" {
  value = { for k, r in aws_ecr_repository.service : k => r.repository_url }
}

output "opensearch_endpoint" {
  value = "https://${aws_opensearch_domain.this.endpoint}"
}

output "opensearch_dashboards_url" {
  value = "https://${aws_opensearch_domain.this.dashboard_endpoint}"
}

output "github_actions_access_key_id" {
  description = "Valor do GitHub Secret AWS_ACCESS_KEY_ID."
  value       = aws_iam_access_key.github_actions.id
  sensitive   = true
}

output "github_actions_secret_access_key" {
  description = "Valor do GitHub Secret AWS_SECRET_ACCESS_KEY."
  value       = aws_iam_access_key.github_actions.secret
  sensitive   = true
}

output "app_secret_name" {
  description = "Segredo no AWS Secrets Manager com todas as credenciais da aplicação."
  value       = aws_secretsmanager_secret.app.name
}