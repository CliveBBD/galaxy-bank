terraform {
  backend "s3" {
    bucket  = "galaxy-bank-s3-bucket"
    key     = "terraform.tfstate"
    region  = "af-south-1"
    encrypt = true
  }
}
