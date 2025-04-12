resource "aws_ecs_cluster" "ecs_cluster" {
  name = "galaxybank-cluster"
}

resource "aws_security_group" "ecs_sg" {
  name        = "ecs-sg"
  description = "Security group for ECS tasks"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_ecr_repository" "ecr_repository" {
  name = "galaxybank-api"
}

resource "aws_ecs_task_definition" "ecs_task_definition" {
  family                   = "galaxybank-task"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = aws_iam_role.ecs_task_role.arn

  container_definitions = jsonencode([
    {
      name  = "galaxybank-api",
      image = "${aws_ecr_repository.ecr_repository.repository_url}:latest",
      #command   = ["/app/GalaxyBank.dll"], #  Corrected command.
      portMappings = [
        {
          containerPort = 80
          hostPort      = 80
        },
      ],
      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT",
          value = "Production"
        },
        {
          name  = "DB_HOST",
          value = aws_db_instance.postgres.endpoint
        },
        {
          name  = "DB_NAME",
          value = "galaxybankdb"
        },
        {
          name  = "DB_USER",
          value = "galaxybank"
        },
        {
          name  = "DB_PASSWORD",
          value = "password"
        },
      ],

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = "/ecs/galaxybank-api"
          awslogs-region        = "af-south-1"
          awslogs-stream-prefix = "ecs"
        }
      }
    },
  ])
}

# -----------------------------------------------------------------------------
# 4. ECS Service (to run and maintain the task)
# -----------------------------------------------------------------------------

resource "aws_ecs_service" "ecs_service" {
  name            = "galaxybank-service"
  cluster         = aws_ecs_cluster.ecs_cluster.id
  task_definition = aws_ecs_task_definition.ecs_task_definition.arn
  desired_count   = 1         #  Start with one task
  launch_type     = "FARGATE" #  Use Fargate
  network_configuration {
    subnets          = [for subnet in aws_subnet.private_subnets : subnet.id] #  Place in private subnets
    security_groups  = [aws_security_group.ecs_sg.id]
    assign_public_ip = false #  Fargate in private subnet
  }

  load_balancer { # Attach to the Application Load Balancer
    target_group_arn = aws_lb_target_group.alb_target_group.arn
    container_name   = "galaxybank-api" #  The name of the container in the task definition
    container_port   = 80
  }
  depends_on = [aws_lb_listener.alb_listener] #  Make sure the ALB is created first.
}