resource "aws_db_subnet_group" "rds_subnet_group" {
  name       = "rds-subnet-group"
  subnet_ids = [for subnet in aws_subnet.private_subnets : subnet.id]
}

resource "aws_security_group" "rds_sg" {
  name        = "rds-sg"
  description = "Security group for RDS PostgreSQL"
  vpc_id      = aws_vpc.main.id

  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_sg.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_db_instance" "postgres" {
  allocated_storage      = 20
  db_subnet_group_name   = aws_db_subnet_group.rds_subnet_group.name
  engine                 = "postgres"
  engine_version         = "17.2"
  instance_class         = "db.t3.micro"
  identifier             = "galaxybankdb"
  password               = "password"
  username               = "galaxybank"
  vpc_security_group_ids = [aws_security_group.rds_sg.id]
  multi_az               = false
  storage_type           = "gp2"

  deletion_protection = false
  skip_final_snapshot = true
  publicly_accessible = false
}
