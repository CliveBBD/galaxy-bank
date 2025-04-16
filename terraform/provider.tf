terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
  backend "s3" {
    bucket         = "galaxy-bank-tf-state1"
    key            = "terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "galaxy-bank-state-lock-table"
  }
}

provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"
}