# Estado de sincronizacion Terraform <-> AWS

Para que Terraform gestione la infraestructura existente sin recrearla es necesario importar cada recurso real al estado (`terraform import`). A continuacion se detalla el progreso actual y lo que falta.

## Recursos sincronizados / creados (18/11/2025)

- **Red**
  - `aws_vpc.main` (`vpc-020bbcab6b221869d`)
  - Subnets: `aws_subnet.public_1`, `public_2`, `private_1`, `private_2`
  - `aws_internet_gateway.main` (`igw-062832569177fee65`)
  - Route tables: `aws_route_table.public`/`private` y sus asociaciones (`public_1/2`, `private_1/2`)
- **Security Groups**
  - `aws_security_group.alb` (`sg-0955ed6b2ed93e01b`)
  - `aws_security_group.ecs` (`sg-03e47660f2aaf76ae`)
  - `aws_security_group.rds` (`sg-0ce093707a2f0c84d`)
  - `aws_security_group.apprunner_connector` (`sg-094756d0d65886fc1`)
- **Application Load Balancer**
  - `aws_lb.main` (`gatekeep-alb`) con tags sincronizados
  - Listener HTTP (`aws_lb_listener.main`) y Listener HTTPS (`aws_lb_listener.https`) validado con ACM
  - Target groups: `aws_lb_target_group.main` (API) y `aws_lb_target_group.frontend`
  - Reglas activas: `backend_api`, `backend_auth`, `backend_usuarios`, `backend_swagger`, `backend_health`
- **Route 53 / HTTPS**
  - Alias A `aws_route53_record.alb_alias[0]` (root) y `[1]` (`www`)
  - Certificado ACM `aws_acm_certificate.alb` + validacion (`aws_acm_certificate_validation.alb`)
  - Registros DNS de validacion (`aws_route53_record.acm_validation`)
- **ECS**
  - `aws_ecs_cluster.main` (`gatekeep-cluster`)
  - Servicios `aws_ecs_service.main` (`gatekeep-api-service`) y `aws_ecs_service.frontend`
  - `aws_ecs_task_definition.main` (API) y `aws_ecs_task_definition.frontend` recreada para apuntar a `https://zimmzimmgames.com`
- **IAM**
  - Roles: `aws_iam_role.ecs_execution`, `aws_iam_role.ecs_task`, `aws_iam_role.rds_monitoring`
  - Policies: `aws_iam_role_policy_attachment.ecs_execution`, `aws_iam_role_policy.ecs_execution_secrets`, `aws_iam_role_policy_attachment.rds_monitoring`, `aws_iam_role_policy.ecs_task_cloudwatch`
- **ECR**
  - `aws_ecr_repository.gatekeep_api` y `aws_ecr_repository.gatekeep_frontend` con lifecycle policies y tags sincronizados
- **Secrets / Parameter Store**
  - `aws_secretsmanager_secret.db_password`, `aws_secretsmanager_secret.jwt_key`
  - Parametros SSM `/gatekeep/db/*` y `/gatekeep/ecr/repository-uri`
- **RDS**
  - `aws_db_subnet_group.main`, `aws_db_parameter_group.postgres16`, `aws_db_instance.main` (`gatekeep-db`)
  - IAM `aws_iam_role.rds_monitoring` y su policy adjunta
- **Observabilidad (CloudWatch)**
  - Dashboard `aws_cloudwatch_dashboard.cache_metrics`
  - Metric filters `cache_hits|misses|removed` en `/ecs/gatekeep`
  - Alarmas `low/critical hit rate`, `high invalidations`, `high misses` y composite alarm `cache_health_overall`

> Nota: Tras el `terraform apply` del 18/11/2025 todo lo listado arriba esta gestionado por Terraform.

## Recursos pendientes / proximos pasos

- **Monitoreo en produccion**
  - Validar que los patrones de los metric filters coincidan con los logs reales. Ajustarlos si cambia el formato.
  - Configurar `var.alarm_actions` con un SNS Topic / webhook para recibir alertas.
- **Secrets**
  - `TF_VAR_manage_secret_versions` permanece en `false` para evitar rotaciones inadvertidas. Activarlo y aplicar solo cuando se planifique un rotate.
- **Costos y optimizacion**
  - Revisar periodicamente el costo de CloudWatch (dashboards, metric filters, alarmas) y del listener HTTPS para mantener el gasto bajo control.
- **Otros**
  - App Runner sigue deshabilitado en `sa-east-1`.
  - Importar/gestionar con Terraform cualquier recurso nuevo (colas, caches, etc.) antes de usarlo en produccion.

## Recomendaciones para continuar

1. Ejecutar `terraform plan` en la misma sesion antes de cada cambio importante para detectar recreaciones o tags inesperados.
2. Mantener `TF_VAR_manage_secret_versions=false` como valor por defecto y habilitarlo unicamente cuando se quiera rotar secretos.
3. Documentar cualquier cambio manual en AWS para volver a importarlo y evitar drift del estado.
