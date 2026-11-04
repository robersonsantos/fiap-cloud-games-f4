terraform {
  required_version = ">= 1.6.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 6.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
  }

  # Estado local (terraform.tfstate fica fora do Git — veja .gitignore).
  # Para trabalho em grupo, troque por um backend S3:
  # backend "s3" { bucket = "<bucket>" key = "fcg/terraform.tfstate" region = "us-east-2" }
}

provider "aws" {
  region = var.region

  default_tags {
    tags = {
      Project   = "fiap-cloud-games"
      Fase      = "4"
      ManagedBy = "terraform"
    }
  }
}

data "aws_caller_identity" "current" {}
data "aws_availability_zones" "available" {
  state = "available"
}

locals {
  name       = var.project
  account_id = data.aws_caller_identity.current.account_id
  azs        = slice(data.aws_availability_zones.available.names, 0, 2)
}