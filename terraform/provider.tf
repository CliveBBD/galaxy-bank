terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
  backend "s3" {
    bucket         = "galaxy-bank-tf-state"
    key            = "terraform.tfstate"
    region         = "af-south-1"
    dynamodb_table = "galaxy-bank-state-lock-table"
  }
}

provider "aws" {
  region = "af-south-1"
}

provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"
}