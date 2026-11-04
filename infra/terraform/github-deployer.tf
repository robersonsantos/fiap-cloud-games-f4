# Identidade usada pelo GitHub Actions para publicar no ECR e fazer deploy no EKS.
#
# O ideal seria OIDC (credenciais temporárias, sem chave guardada), mas a organização AWS
# desta conta bloqueia iam:CreateOpenIDConnectProvider por service control policy.
# Alternativa: um usuário IAM exclusivo da pipeline, com permissões mínimas, cuja chave
# fica somente nos GitHub Secrets (criptografados) — nunca no código.
resource "aws_iam_user" "github_actions" {
  name = "${local.name}-github-actions"
}

data "aws_iam_policy_document" "github_permissions" {
  statement {
    sid       = "EcrLogin"
    actions   = ["ecr:GetAuthorizationToken"]
    resources = ["*"]
  }

  statement {
    sid = "EcrPush"
    actions = [
      "ecr:BatchCheckLayerAvailability",
      "ecr:BatchGetImage",
      "ecr:CompleteLayerUpload",
      "ecr:DescribeImages",
      "ecr:DescribeImageScanFindings",
      "ecr:GetDownloadUrlForLayer",
      "ecr:InitiateLayerUpload",
      "ecr:PutImage",
      "ecr:StartImageScan",
      "ecr:UploadLayerPart",
    ]
    resources = [for r in aws_ecr_repository.service : r.arn]
  }

  statement {
    sid       = "EksDescribe"
    actions   = ["eks:DescribeCluster"]
    resources = [aws_eks_cluster.this.arn]
  }

  statement {
    sid       = "ReadAppSecrets"
    actions   = ["secretsmanager:GetSecretValue", "secretsmanager:DescribeSecret"]
    resources = [aws_secretsmanager_secret.app.arn]
  }
}

resource "aws_iam_user_policy" "github_actions" {
  name   = "deploy"
  user   = aws_iam_user.github_actions.name
  policy = data.aws_iam_policy_document.github_permissions.json
}

# A chave fica no terraform.tfstate (local, fora do Git) e é copiada para os GitHub Secrets.
resource "aws_iam_access_key" "github_actions" {
  user = aws_iam_user.github_actions.name
}