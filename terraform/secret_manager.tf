resource "aws_secretsmanager_secret" "db_connection" {
  name = "${var.app_name}-db-connection-v3"
}

resource "aws_secretsmanager_secret_version" "db_connection_version" {
  secret_id     = aws_secretsmanager_secret.db_connection.id
  secret_string = "Host=${aws_db_instance.postgres.address};Port=5432;Database=${var.db_name};Username=${var.db_username};Password=${var.db_password}"
}

resource "aws_secretsmanager_secret" "google_client_id" {
  name        = "${var.app_name}/google-client-id-v3"
  description = "Google OAuth Client ID"
}

resource "aws_secretsmanager_secret" "google_client_secret" {
  name        = "${var.app_name}/google-client-secret-v3"
  description = "Google OAuth Client Secret"
}

resource "aws_secretsmanager_secret" "google_redirect_uri" {
  name        = "${var.app_name}/google-client-redirect-uri-v3"
  description = "Google OAuth Redirect URI"
}

resource "aws_secretsmanager_secret" "email_settings_username" {
  name        = "email-settings-username"
  description = "Email settings username"
}

resource "aws_secretsmanager_secret" "email_settings_password" {
  name        = "email-settings-password"
  description = "Email settings password"
}
