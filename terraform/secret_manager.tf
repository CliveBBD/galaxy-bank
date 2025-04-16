resource "aws_secretsmanager_secret" "db_connection" {
  name = "${var.app_name}-db-connection-v3"
}

resource "aws_secretsmanager_secret_version" "db_connection_version" {
  secret_id     = aws_secretsmanager_secret.db_connection.id
  secret_string = "Host=${aws_db_instance.postgres.address};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${var.db_password}"
}
