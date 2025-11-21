# Configuración DNS para zimmzimmgames.com

## 📋 Records DNS Necesarios

### 1. A Records (Apuntan al servidor)

```
Type: A
Name: @
Value: <TU_IP_PUBLICA>
TTL: 300 (o automático)

Type: A
Name: www
Value: <TU_IP_PUBLICA>
TTL: 300

Type: A
Name: api
Value: <TU_IP_PUBLICA>
TTL: 300
```

**Reemplazar `<TU_IP_PUBLICA>` con tu dirección IP pública o IP del ALB**

### 2. CAA Records (Autorización de Certificados)

```
Type: CAA
Name: @
Flags: 0
Tag: issue
Value: "letsencrypt.org"

Type: CAA
Name: @
Flags: 0
Tag: issuewild
Value: "letsencrypt.org"

Type: CAA
Name: @
Flags: 0
Tag: iodef
Value: "mailto:tu-email@example.com"
```

### 3. MX Records (Correo - Opcional)

Si necesitas recibir emails:

```
Type: MX
Name: @
Priority: 10
Value: mail.zimmzimmgames.com

Type: MX
Name: @
Priority: 20
Value: mail2.zimmzimmgames.com
```

### 4. TXT Records (SPF/DKIM - Opcional)

Para validación de emails:

```
Type: TXT
Name: @
Value: "v=spf1 include:sendgrid.net ~all"
```

---

## 🔧 Cómo Configurar en Proveedores Populares

### GoDaddy

1. Ir a "My Products" → "Domain" → "Manage"
2. Click en "DNS"
3. Agregar A records bajo "DNS Management"
4. Agregar CAA records
5. Guardar cambios

### Namecheap

1. Dashboard → "Domain List"
2. Click "Manage" en zimmzimmgames.com
3. Ir a "Advanced DNS"
4. Agregar A records
5. Agregar CAA records

### Route 53 (AWS)

```bash
# Puedes configurar via AWS CLI
aws route53 change-resource-record-sets \
  --hosted-zone-id Z1234567890ABC \
  --change-batch '{
    "Changes": [
      {
        "Action": "CREATE",
        "ResourceRecordSet": {
          "Name": "zimmzimmgames.com",
          "Type": "A",
          "TTL": 300,
          "ResourceRecords": [{"Value": "TU_IP_PUBLICA"}]
        }
      }
    ]
  }'
```

### Cloudflare

1. Agregar dominio
2. Ir a "DNS"
3. Agregar registros A para:
   - @ (raíz)
   - www
   - api
4. Configurar CAA records
5. Verificar propagación

---

## ✅ Verificar Configuración DNS

### Verificar que los registros estén propagados

```bash
# Verificar A record principal
dig zimmzimmgames.com A

# Verificar www
dig www.zimmzimmgames.com A

# Verificar api
dig api.zimmzimmgames.com A

# Verificar CAA
dig zimmzimmgames.com CAA
```

### Usando nslookup

```bash
# Windows o cualquier sistema
nslookup zimmzimmgames.com
nslookup www.zimmzimmgames.com
nslookup api.zimmzimmgames.com
```

### Propagación Global

Verificar propagación en:
https://www.whatsmydns.net/

---

## 🌍 Propagación DNS

**Nota:** Los cambios DNS pueden tardar de 1 a 48 horas en propagarse globalmente.

Para acelerar:
1. Reducir TTL a 300 segundos antes de cambios
2. Esperar a que se propague
3. Después, puede aumentar a 3600 o más

---

## 🔐 Seguridad DNS

### DNSSEC

Si tu proveedor lo soporta:

```bash
# Habilitar DNSSEC en tu DNS provider
# Verificar:
dig zimmzimmgames.com +dnssec
```

### DNS Monitoring

Monitorear cambios DNS con:
```bash
# Configurar alertas en:
https://www.cloudflare.com/
https://www.ns1.com/
https://www.route53.aws/
```

---

## 📝 Checklist

- [ ] A record para @ apuntando a IP pública
- [ ] A record para www apuntando a IP pública
- [ ] A record para api apuntando a IP pública
- [ ] CAA records configurados para Let's Encrypt
- [ ] TTL configurado (300-3600 segundos)
- [ ] Verificar propagación con `dig`
- [ ] DNSSEC habilitado (opcional)
- [ ] Monitoreo configurado (opcional)

---

**Dominio:** zimmzimmgames.com  
**Última actualización:** 18 de Noviembre 2025
