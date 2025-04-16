resource "aws_ecs_cluster" "main" {
  name = "${var.app_name}-cluster"
}

resource "aws_ecs_task_definition" "api" {
  family                   = "${var.app_name}-task"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "512"
  memory                   = "1024"
  network_mode             = "awsvpc"
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn
  task_role_arn            = aws_iam_role.ecs_task_role.arn

  runtime_platform {
    operating_system_family = "LINUX"
    cpu_architecture        = "X86_64"
  }

  container_definitions = jsonencode([
    {
      name  = var.app_name
      image = "${aws_ecr_repository.api.repository_url}:latest"
      portMappings = [
        {
          containerPort = 80,
          hostPort      = 80,
          protocol      = "tcp"
        }
      ]
      environment = [
        {
          name  = "ASPNETCORE_URLS",
          value = "http://+:80"
        },
        {
          name  = "ASPNETCORE_ENVIRONMENT",
          value = "Development"
        },
        {
          name  = "DEFAULT_CONNECTION_STRING",
          value = "Host=${aws_db_instance.postgres.endpoint},Port=${aws_db_instance.postgres.port};Database=${aws_db_instance.postgres.db_name};Username=${aws_db_instance.postgres.username};Password=${aws_db_instance.postgres.password};"
        },
      ]
      secrets = [
        {
          name      = "ConnectionStrings__DefaultConnection"
          valueFrom = aws_secretsmanager_secret.db_connection.arn
        },
        {
          name      = "GoogleClientId"
          valueFrom = data.aws_secretsmanager_secret_version.google_client_id.secret_string
        },
        {
          name      = "GoogleClientSecret"
          valueFrom = data.aws_secretsmanager_secret_version.google_client_secret.secret_string
        },
        {
          name      = "GoogleRedirectUri"
          valueFrom = data.aws_secretsmanager_secret_version.google_redirect_uri.secret_string
        }
      ]
      logConfiguration = {
        logDriver = "awslogs",
        options = {
          "awslogs-group"         = "/ecs/${var.app_name}",
          "awslogs-region"        = var.region,
          "awslogs-stream-prefix" = "ecs"
        }
      }
      essential = true
    }
  ])
}

resource "aws_ecs_service" "api" {
  name            = "${var.app_name}-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.api.arn
  launch_type     = "FARGATE"
  desired_count   = 1

  network_configuration {
    subnets          = [aws_subnet.public_subnet1.id, aws_subnet.public_subnet2.id]
    security_groups  = [aws_security_group.ecs_sg.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name   = var.app_name
    container_port   = 80
  }

  depends_on = [aws_lb_listener.http]
}

resource "aws_cloudwatch_log_group" "ecs_logs" {
  name              = "/ecs/${var.app_name}-logs"
  retention_in_days = 7
}
