# 📋 Resumen de Cambios Necesarios

**Fecha:** 2025-01-21  
**Prioridad:** 🔴 Alta

---

## 🎯 CAMBIO ÚNICO REQUERIDO

### Variable de Entorno en Task Definition del Frontend

**Recurso AWS:**  
- ECS Task Definition
- Familia: `gatekeep-frontend`
- Revisión actual: `3`
- Revisión nueva: `4` (se creará)

**Variable a cambiar:**
```
NEXT_PUBLIC_API_URL
```

**Valor actual:** ❌  
```
https://zimmzimmgames.com
```

**Valor correcto:** ✅  
```
https://api.zimmzimmgames.com
```

---

## 📝 PASOS A EJECUTAR

1. **Obtener Task Definition actual**
   ```bash
   aws ecs describe-task-definition --task-definition gatekeep-frontend:3 --region sa-east-1
   ```

2. **Modificar variable** en el JSON obtenido

3. **Registrar nueva revisión**
   ```bash
   aws ecs register-task-definition --cli-input-json file://task-definition-new.json --region sa-east-1
   ```

4. **Actualizar servicio ECS**
   ```bash
   aws ecs update-service --cluster gatekeep-cluster --service gatekeep-frontend-service --task-definition gatekeep-frontend:4 --region sa-east-1 --force-new-deployment
   ```

5. **Verificar deployment** (esperar 2-5 minutos)

---

## ⚠️ IMPACTO

- **Downtime:** Mínimo (2-5 minutos durante rolling deployment)
- **Riesgo:** Bajo (hay plan de rollback)
- **Beneficio:** Frontend funcionará correctamente en todos los escenarios (cliente + SSR)

---

## ✅ RESULTADO ESPERADO

Después del cambio:
- ✅ Variable `NEXT_PUBLIC_API_URL` correcta
- ✅ Frontend puede hacer llamadas al backend correctamente
- ✅ SSR funcionará correctamente
- ✅ No habrá errores 404 por URL incorrecta

---

**Última actualización:** 2025-01-21

