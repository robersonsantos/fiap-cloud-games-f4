# Role somente leitura para navegar no Console AWS (demonstrações e vídeo).
# Vê EKS (inclusive nós e pods), ECR, Load Balancers, OpenSearch e Secrets Manager,
# mas não altera nada e não lê o valor dos segredos (ReadOnlyAccess não inclui GetSecretValue).
data "aws_iam_policy_document" "console_viewer_assume" {
  statement {
    actions = ["sts:AssumeRole"]
    principals {
      type        = "AWS"
      identifiers = ["arn:aws:iam::${local.account_id}:root"]
    }
  }
}

resource "aws_iam_role" "console_viewer" {
  name                 = "${local.name}-console-viewer"
  assume_role_policy   = data.aws_iam_policy_document.console_viewer_assume.json
  max_session_duration = 14400
}

resource "aws_iam_role_policy_attachment" "console_viewer" {
  role       = aws_iam_role.console_viewer.name
  policy_arn = "arn:aws:iam::aws:policy/ReadOnlyAccess"
}

resource "aws_eks_access_entry" "console_viewer" {
  cluster_name  = aws_eks_cluster.this.name
  principal_arn = aws_iam_role.console_viewer.arn
}

resource "aws_eks_access_policy_association" "console_viewer" {
  cluster_name  = aws_eks_cluster.this.name
  principal_arn = aws_iam_role.console_viewer.arn
  policy_arn    = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSAdminViewPolicy"

  access_scope {
    type = "cluster"
  }

  depends_on = [aws_eks_access_entry.console_viewer]
}
