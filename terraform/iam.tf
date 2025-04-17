data "aws_caller_identity" "current" {}

locals {
  runner = "${var.app_name}-runner"
}

resource "aws_iam_openid_connect_provider" "github" {
  url             = "https://token.actions.githubusercontent.com"
  client_id_list  = ["sts.amazonaws.com"]
  thumbprint_list = ["6938fd4d98bab03faadb97b34396831e3780aea1"]
}

resource "aws_iam_role" "github_actions" {
  name = "GitHubActionsOIDCRole"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Principal = {
          Federated = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:oidc-provider/token.actions.githubusercontent.com"
        },
        Action = "sts:AssumeRoleWithWebIdentity",
        Condition = {
          StringEquals = {
            "token.actions.githubusercontent.com:aud" = "sts.amazonaws.com"
          },
          StringLike = {
            "token.actions.githubusercontent.com:sub" = "repo:CliveBBD/galaxy-bank:*"
          }
        }
      }
    ]
  })
}

data "aws_iam_policy_document" "ecs_task_execution_policy" {
  statement {
    actions = [
      "secretsmanager:GetSecretValue",
      "ssm:GetParameters",
      "ssm:GetParameter"
    ]
    resources = [
      aws_secretsmanager_secret.db_connection.arn
    ]
  }
}

data "aws_iam_policy_document" "list_tags_for_resource_policy" {
  statement {
    actions = [
      "*",
    ]
    resources = [
      "*"
    ]
  }
}

resource "aws_iam_role_policy" "ecs_task_execution_secrets" {
  name   = "${var.app_name}-task-execution-secrets-policy"
  role   = aws_iam_role.ecs_task_execution.id
  policy = data.aws_iam_policy_document.ecs_task_execution_policy.json
}

resource "aws_iam_role" "ecs_task_execution" {
  name = "${var.app_name}-ecs-task-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        },
        Action = "sts:AssumeRole"
      }
    ]
  })
}

resource "aws_iam_policy" "ecs_task_secrets_policy" {
  name = "ecs-task-execution-role-secrets-policy"
  policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "secretsmanager:GetSecretValue",
          "secretsmanager:DescribeSecret",
          "secretsmanager:GetResourcePolicy",
          "secretsmanager:ListSecretVersionIds"
        ],
        Resource = [
          aws_secretsmanager_secret.google_client_secret.arn,
          aws_secretsmanager_secret.google_client_id.arn,
          aws_secretsmanager_secret.google_redirect_uri.arn
        ]
      },
      {
        Effect   = "Allow",
        Action   = "secretsmanager:ListSecrets",
        Resource = "*"
      }
    ]
  })
}

resource "aws_iam_role" "ecs_task_role" {
  name = "${var.app_name}-task-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
      {
        Effect = "Allow",
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        },
        Action = "sts:AssumeRole"
      }
    ]
  })
}

resource "aws_iam_policy" "ecs_task_s3_read_only" {
  name        = "${var.app_name}-task-s3-read-only-policy"
  description = "Allows read-only access to the Terraform state S3 bucket"
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:ListBucket"
        ],
        Resource = [
          "arn:aws:s3:::galaxy-bank-s3-bucket",
          "arn:aws:s3:::galaxy-bank-s3-bucket/*"
        ]
      }
    ]
  })
}

resource "aws_iam_policy" "terraform_state_access" {
  name        = "TerraformStateAccess"
  description = "Allow full access to the Terraform state bucket"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow",
        Action = [
          "s3:ListBucket",
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject"
        ],
        Resource = [
          "arn:aws:s3:::galaxy-bank-s3-bucket",
          "arn:aws:s3:::galaxy-bank-s3-bucket/*"
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ecr" {
  role       = aws_iam_role.github_actions.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser"
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_policy" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_secrets_policy" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:policy/ecs-task-execution-role-secrets-policy"
}

resource "aws_iam_role_policy_attachment" "ecs" {
  role       = aws_iam_role.github_actions.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonECS_FullAccess"
}

resource "aws_iam_role_policy_attachment" "rds" {
  role       = aws_iam_role.github_actions.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonRDSFullAccess"
}

resource "aws_iam_role_policy_attachment" "ecs_task_s3_attach" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = aws_iam_policy.ecs_task_s3_read_only.arn
}

resource "aws_iam_role_policy_attachment" "github_actions_terraform_state" {
  role       = aws_iam_role.github_actions.name
  policy_arn = aws_iam_policy.terraform_state_access.arn
}

resource "aws_iam_role_policy" "gh_runner_list_tags" {
  name   = "${local.runner}-list-tags-policy"
  role   = aws_iam_role.github_actions.id
  policy = data.aws_iam_policy_document.list_tags_for_resource_policy.json
}