# Amazon OpenSearch Service — motor de busca gerenciado (fuzzy search + relevância).
# Fine-grained access control com usuário mestre (a senha é gerada e guardada no Secrets Manager).
resource "random_password" "opensearch" {
  length           = 24
  min_upper        = 2
  min_lower        = 2
  min_numeric      = 2
  min_special      = 2
  override_special = "_-#"
}

resource "aws_opensearch_domain" "this" {
  domain_name    = "${local.name}-search"
  engine_version = var.opensearch_engine_version

  cluster_config {
    instance_type          = var.opensearch_instance_type
    instance_count         = 1
    zone_awareness_enabled = false
  }

  ebs_options {
    ebs_enabled = true
    volume_type = "gp3"
    volume_size = 10
  }

  encrypt_at_rest {
    enabled = true
  }

  node_to_node_encryption {
    enabled = true
  }

  domain_endpoint_options {
    enforce_https       = true
    tls_security_policy = "Policy-Min-TLS-1-2-2019-07"
  }

  advanced_security_options {
    enabled                        = true
    internal_user_database_enabled = true

    master_user_options {
      master_user_name     = "fcgadmin"
      master_user_password = random_password.opensearch.result
    }
  }

  # O controle de acesso é feito pelo fine-grained access control (usuário/senha);
  # a policy de recurso apenas libera o tráfego HTTPS até o domínio.
  access_policies = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = { AWS = "*" }
      Action    = "es:ESHttp*"
      Resource  = "arn:aws:es:${var.region}:${local.account_id}:domain/${local.name}-search/*"
    }]
  })
}