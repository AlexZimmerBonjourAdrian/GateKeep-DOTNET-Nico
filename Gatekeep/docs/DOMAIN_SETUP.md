# Configuración de `zimmzimmgames.com` con ALB

Esta guía resume los pasos necesarios para exponer GateKeep detrás del dominio público `zimmzimmgames.com` usando el Application Load Balancer (ALB) gestionado por Terraform.

## 1. Prerrequisitos

- Zona pública de Route 53 llamada `zimmzimmgames.com` (con delegación desde el registrador).
- Certificados DNS administrados por la misma cuenta de AWS (Terraform los crea automáticamente).
- Infraestructura creada con los módulos Terraform del directorio `Gatekeep/terraform`.

## 2. Variables relevantes

En `terraform/variables.tf` se añadieron:

- `domain_name`: dominio raíz (por defecto `zimmzimmgames.com`).
- `alternate_domain_names`: lista de SAN, p. ej. `["www.zimmzimmgames.com"]`.

Puedes sobreescribirlas con un archivo `terraform.tfvars`:

```
domain_name             = "zimmzimmgames.com"
alternate_domain_names  = ["www.zimmzimmgames.com"]
```

## 3. Pasos de despliegue

1. Posiciónate en `Gatekeep/terraform`.
2. Ejecuta:

   ```
   terraform init
   terraform plan -out tfplan
   terraform apply tfplan
   ```

   > Nota: Si Terraform no está instalado en tu entorno local, instálalo o ejecuta los comandos desde una máquina con la CLI disponible.

Durante el `apply` se crean:

- Certificado ACM en `sa-east-1` validado vía DNS.
- Listener HTTPS (443) en el ALB usando ese certificado.
- Listener HTTP (80) que redirige automáticamente a HTTPS.
- Registros `A` con alias para cada dominio configurado (`zimmzimmgames.com`, `www.zimmzimmgames.com`).

## 4. Verificación

Tras la propagación DNS (hasta 15 min):

1. `nslookup zimmzimmgames.com` debe devolver el ALB.
2. `curl -I https://zimmzimmgames.com/login` debe responder 200/302 desde el frontend.
3. `curl -I https://zimmzimmgames.com/api/health` debe responder 200 desde la API.

La consola de Route 53 mostrará los nuevos registros A alias y los CNAME de validación de ACM. El listener HTTPS será visible en la consola de EC2 → Load Balancers.

## 5. Consideraciones adicionales

- Para agregar más subdominios (ej. `admin.zimmzimmgames.com`), añádelos a `alternate_domain_names` y vuelve a ejecutar `terraform apply`.
- Si cambias de dominio, actualiza `domain_name`, vuelve a desplegar y ajusta la delegación en el registrador.
- El frontend consume `NEXT_PUBLIC_API_URL=https://zimmzimmgames.com`, por lo que todas las llamadas pasarán por el ALB utilizando HTTPS.

