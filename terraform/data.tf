data "aws_secretsmanager_secret_version" "google_client_id" {
  secret_id = aws_secretsmanager_secret.google_client_id.id
}

data "aws_secretsmanager_secret_version" "google_client_secret" {
  secret_id = aws_secretsmanager_secret.google_client_secret.id
}

data "aws_secretsmanager_secret_version" "google_redirect_uri" {
  secret_id = aws_secretsmanager_secret.google_client_secret.id
}

data "aws_secretsmanager_secret_version" "email_password" {
  secret_id = aws_secretsmanager_secret.email_settings_password.id
}

data "aws_cloudfront_cache_policy" "caching_policy" {
  name = "Managed-CachingDisabled"
}

data "aws_cloudfront_origin_request_policy" "all_viewer" {
  name = "Managed-AllViewer"
}