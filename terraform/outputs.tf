output "alb_dns_name" {
  value       = aws_lb.alb.dns_name
  description = "The DNS name of the Application Load Balancer"
}

output "ecr_repository_url" {
  value       = aws_ecrpublic_repository.ecr_repository.repository_uri
  description = "The URI of the public ECR repository"
}

output "ecs_cluster_name" {
  value       = aws_ecs_cluster.ecs_cluster.name
  description = "The name of the ECS cluster"
}

output "ecs_service_name" {
  value       = aws_ecs_service.ecs_service.name
  description = "The name of the ECS service"
}