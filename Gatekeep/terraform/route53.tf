locals {
  alternate_domains = [
    for domain in var.alternate_domain_names : trimspace(domain)
    if length(trimspace(domain)) > 0 && trimspace(domain) != var.domain_name
  ]

  all_public_domains = distinct(concat([var.domain_name], local.alternate_domains))
}

data "aws_route53_zone" "primary" {
  name         = "${var.domain_name}."
  private_zone = false
}

resource "aws_acm_certificate" "alb" {
  count = var.enable_https ? 1 : 0

  domain_name               = var.domain_name
  subject_alternative_names = local.alternate_domains
  validation_method         = "DNS"

  lifecycle {
    create_before_destroy = true
  }

  tags = {
    Name        = "${var.project_name}-alb-cert"
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

resource "aws_route53_record" "acm_validation" {
  for_each = var.enable_https ? {
    for dvo in aws_acm_certificate.alb[0].domain_validation_options :
    dvo.domain_name => {
      name   = dvo.resource_record_name
      type   = dvo.resource_record_type
      record = dvo.resource_record_value
    }
  } : {}

  allow_overwrite = true
  name            = each.value.name
  records         = [each.value.record]
  ttl             = 60
  type            = each.value.type
  zone_id         = data.aws_route53_zone.primary.zone_id
}

resource "aws_acm_certificate_validation" "alb" {
  count = var.enable_https ? 1 : 0

  certificate_arn         = aws_acm_certificate.alb[0].arn
  validation_record_fqdns = [for record in aws_route53_record.acm_validation : record.fqdn]
}

resource "aws_route53_record" "alb_alias" {
  count = length(local.all_public_domains)

  zone_id = data.aws_route53_zone.primary.zone_id
  name    = local.all_public_domains[count.index]
  type    = "A"

  alias {
    name                   = aws_lb.main.dns_name
    zone_id                = aws_lb.main.zone_id
    evaluate_target_health = true
  }
}

