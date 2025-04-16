output "ecr_repository_url" {
  value       = aws_ecr_repository.api.repository_url
  description = "The URL of the ECR repository"
}

output "ecs_cluster_name" {
  value       = aws_ecs_cluster.main.name
  description = "The name of the ECS cluster"
}

output "ecs_service_name" {
  value       = aws_ecs_service.api.name
  description = "The name of the ECS service"
}

output "rds_endpoint" {
  value       = aws_db_instance.postgres.endpoint
  description = "The endpoint of the RDS instance"
}

output "alb_dns_name" {
  value       = aws_lb.api.dns_name
  description = "The DNS name of the load balancer"
}

