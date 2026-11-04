# Registry privado nativo da AWS (Amazon ECR) — um repositório por microsserviço.
locals {
  services = ["users-api", "catalog-api", "payments-api", "notifications-api"]
}

resource "aws_ecr_repository" "service" {
  for_each = toset(local.services)

  name = "${local.name}/${each.key}"
  # Tags imutáveis: cada imagem é identificada pelo SHA do commit, garantindo rastreabilidade.
  image_tag_mutability = "IMMUTABLE"
  force_delete         = true

  image_scanning_configuration {
    scan_on_push = true
  }
}

resource "aws_ecr_lifecycle_policy" "service" {
  for_each   = aws_ecr_repository.service
  repository = each.value.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Mantém apenas as 15 imagens mais recentes"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 15
      }
      action = { type = "expire" }
    }]
  })
}