resource "aws_db_subnet_group" "main" {
  name       = "${var.app_name}-subnet"
  subnet_ids = [aws_subnet.public_subnet1.id, aws_subnet.public_subnet2.id]
}

resource "aws_db_instance" "postgres" {
  identifier              = var.app_name
  engine                  = "postgres"
  engine_version          = "17.4"
  instance_class          = "db.t3.micro"
  allocated_storage       = 20
  port                    = var.db_port
  username                = var.db_username
  password                = var.db_password
  db_name                 = var.db_name
  backup_retention_period = 7
  backup_window           = "03:00-04:00"
  maintenance_window      = "sun:05:00-sun:06:00"
  skip_final_snapshot     = true
  publicly_accessible     = true
  db_subnet_group_name    = aws_db_subnet_group.main.name
  vpc_security_group_ids  = [aws_security_group.rds_sg.id]
}
