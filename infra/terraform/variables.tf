variable "region" {
  description = "Região AWS. A organização desta conta só libera us-east-2 (Ohio)."
  type        = string
  default     = "us-east-2"
}

variable "project" {
  description = "Prefixo dos recursos (cluster, ECR, OpenSearch, secrets)."
  type        = string
  default     = "fcg"
}

variable "kubernetes_version" {
  description = "Versão do Kubernetes no EKS (use uma versão em suporte padrão para evitar a cobrança de suporte estendido)."
  type        = string
  default     = "1.36"
}

variable "node_instance_type" {
  description = "Tipo das instâncias do node group."
  type        = string
  default     = "t3.medium"
}

variable "node_capacity_type" {
  description = "ON_DEMAND ou SPOT (SPOT custa ~70% menos, mas pode ser interrompida)."
  type        = string
  default     = "ON_DEMAND"
}

variable "node_desired_size" {
  description = "Quantidade de nós."
  type        = number
  default     = 2
}

variable "opensearch_instance_type" {
  description = "Instância do Amazon OpenSearch (t3.small.search é a menor com fine-grained access control)."
  type        = string
  default     = "t3.small.search"
}

variable "opensearch_engine_version" {
  description = "Versão do engine do Amazon OpenSearch."
  type        = string
  default     = "OpenSearch_2.19"
}

variable "admin_email" {
  description = "E-mail do administrador inicial da plataforma (a senha é gerada e fica no Secrets Manager)."
  type        = string
  default     = "admin@fcg.com"
}

variable "budget_limit_usd" {
  description = "Limite mensal (USD) do AWS Budget de alerta."
  type        = number
  default     = 30
}

variable "budget_email" {
  description = "E-mail que recebe os alertas de custo. Vazio = não cria o budget."
  type        = string
  default     = ""
}