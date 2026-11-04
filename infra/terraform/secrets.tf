# AWS Secrets Manager — fonte única das credenciais da aplicação.
# Todas as senhas são geradas aqui; nada é digitado à mão nem versionado.
# A pipeline (GitHub Actions, usuário IAM fcg-github-actions) lê este segredo e o sincroniza no Secret
# "fcg-secrets" do Kubernetes, que os pods consomem em tempo de execução.

resource "random_password" "jwt" {
  length  = 64
  special = false
}

resource "random_password" "sql" {
  length           = 24
  min_upper        = 2
  min_lower        = 2
  min_numeric      = 2
  min_special      = 2
  override_special = "_-#"
}

resource "random_password" "alnum" {
  for_each = toset(["rabbitmq", "mongo", "redis", "grafana"])

  length  = 32
  special = false
}

resource "random_password" "admin" {
  length           = 16
  min_upper        = 2
  min_lower        = 2
  min_numeric      = 2
  min_special      = 2
  override_special = "@#_-"
}

resource "aws_secretsmanager_secret" "app" {
  name                    = "${local.name}/app"
  description             = "Credenciais da FIAP Cloud Games (injetadas no Kubernetes pela pipeline)."
  recovery_window_in_days = 0
}

resource "aws_secretsmanager_secret_version" "app" {
  secret_id = aws_secretsmanager_secret.app.id

  secret_string = jsonencode({
    JWT_KEY             = random_password.jwt.result
    SA_PASSWORD         = random_password.sql.result
    RABBITMQ_USER       = "fcg"
    RABBITMQ_PASSWORD   = random_password.alnum["rabbitmq"].result
    MONGO_USER          = "fcg"
    MONGO_PASSWORD      = random_password.alnum["mongo"].result
    REDIS_PASSWORD      = random_password.alnum["redis"].result
    GRAFANA_PASSWORD    = random_password.alnum["grafana"].result
    OPENSEARCH_URL      = "https://${aws_opensearch_domain.this.endpoint}"
    OPENSEARCH_USER     = "fcgadmin"
    OPENSEARCH_PASSWORD = random_password.opensearch.result
    ADMIN_EMAIL         = var.admin_email
    ADMIN_PASSWORD      = random_password.admin.result
  })
}