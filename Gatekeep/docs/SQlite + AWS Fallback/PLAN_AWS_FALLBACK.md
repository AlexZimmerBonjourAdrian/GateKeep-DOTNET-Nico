# Plan de Implementación: AWS - Fallback offline para PWA + sql.js

**Fecha:** 2025-11-18
**Propósito:** Plan operacional y técnico para desplegar la PWA (sql.js) en AWS con fallback offline resiliente. Formato y nivel de detalle alineado a `PLAN_PERSISTENCIA_DATOS.md`.

---

## 📋 Resumen ejecutivo
- Objetivo: Garantizar que la PWA funcione offline y que, ante caídas del backend/origen, CloudFront sirva un fallback (`offline.html`) mientras los eventos offline se encolan (SQS) y se procesan de forma idempotente por un worker.
- Resultado esperado: PWA usable en offline, sincronización fiable (deviceId + idTemporal), assets WASM y SW servidos correctamente por CDN, y pipeline resiliente en AWS (S3, CloudFront, SQS, Secrets Manager, worker).

---

## ⚙️ Asunciones
- Existe infra base en Terraform (proveedor AWS configurado).
- Backend .NET ya corre contra RDS Postgres, Redis y MongoDB/DocumentDB.
- Frontend Next.js genera build estático y artefactos (`sw.js`, `offline.html`, `sql-wasm.wasm`).
- Equipo usa CI/CD que puede ejecutar Terraform apply y subir assets a S3.

---

## 🗺️ Arquitectura propuesta (alto nivel)

```
[Browser PWA] -- CloudFront (CDN, fallback) -- S3 (static)        (API calls -> ALB/ECS or API GW)
      |                                  \                     / 
      |                                   \-> /api/sync -> ALB -> ECS (.NET) -> SQS enqueue
      |                                                                     |
      |                                                                     v
      |                                                                SQS Queue
      |                                                                     |
      |                                                                     v
      |                                                               Worker (Lambda/ECS)
      |                                                                     |
      |                                                                     v
      |                                                        Postgres (RDS) + MongoDB (audit)
```

---

## 🎯 Objetivos técnicos (acotados)
- Servir `sql-wasm.wasm`, `sw.js`, `offline.html` y assets críticos con cabeceras correctas desde S3+CloudFront.
- CloudFront devuelve `offline.html` ante 5xx/502/503/504 del origen (TTL corto).
- Endpoint `POST /api/sync/batch` encola lotes en SQS y devuelve 202; worker procesa, aplica idempotencia y persiste.
- Idempotencia garantizada por `deviceId + idTemporal` y mapping `idTemporal -> idServidor`.
- Observabilidad: métricas y alarmas CloudWatch (SQS depth, 5xx rate, DLQ).
- Seguridad: HTTPS, WAF básico, CSP, Secrets Manager para VAPID/JWKS, roles IAM mínimos.

---

## 📌 Plan de trabajo (fases)

FASE 0 — Preparación (0.5-1 día)
- Revisar Terraform existente y CI/CD.
- Añadir bucket de staging si hace falta.
- Definir nombres/convenciones para recursos (S3, CF, SQS, Secrets).

FASE 1 — Infra Terraform (1-2 días)
- Módulo S3: `frontend_bucket` (versioning, OAC/OAI, CORS, metadata wasm).
- Módulo CloudFront: distribution con behaviors y Custom Error Responses a `offline.html`.
- Módulo SQS: `gatekeep-sync-queue` + DLQ + redrive policy.
- Secrets Manager entries: `gatekeep/vapid`, `gatekeep/jwks` (si aplica).
- IAM roles/policies: backend (SendMessage), worker (Consume SQS + Secrets). 
- Test: deploy en staging (terraform plan/apply) y validar recursos creados.

FASE 2 — Backend (.NET) (1-2 días)
- API contract: `POST /api/sync/batch` (deviceId, ultimaActualizacion, eventos[], metadata).
- Implementar lógica de encolado a SQS y respuesta 202 (syncId opcional).
- Validaciones: JWT/Cognito, tamaño lote, permisos sobre usuarioId.
- Añadir timestamps en entidades si faltan (`FechaCreacion`, `UltimaActualizacion`).
- Instrumentar métricas CloudWatch (enqueue count, 5xx rate).
- Test: integración local con SQS (o mock) + unit tests.

FASE 3 — Worker (1-2 días)
- Implementar consumidor (Lambda o servicio .NET): recibir mensajes, aplicar idempotencia, persistir EventoAcceso y auditoría en MongoDB.
- Manejo de errores: reintentos, DLQ y logging estructurado.
- Metric hooks: processing latency, success/failure counters.
- Test: integración con SQS y RDS/Mongo en staging.

FASE 4 — Frontend (PWA) (1-2 días)
- Build: asegurar `sql-wasm.wasm` y `sw.js` generados y `offline.html` presente.
- Upload a S3 con `Content-Type` correcto (`application/wasm` para .wasm) y CORS configurado.
- CloudFront invalidation para assets críticos tras deploy.
- Confirmar SW registrado y WASM cargado desde CDN.
- UX: indicador online/offline, contador pendientes, botón "Reintentar sincronización".
- Test: simular offline en DevTools, registrar eventos en SQLite y reconectar.

FASE 5 — Observabilidad, seguridad y rollout (1 día)
- Crear dashboards y alarmas en CloudWatch.
- Configurar WAF básico y CSP en headers.
- Pipeline CI: build frontend -> upload S3 -> invalidate CF; build backend -> deploy -> smoke tests.
- Rollout por etapas: staging → canary → prod.

---

## ✅ Checklist detallada (operativa)

Infra (Terraform)
- [ ] `frontend_bucket` (S3) con versioning, OAC/OAI y CORS
- [ ] `cloudfront_distribution` con Custom Error Responses -> `offline.html`
- [ ] SQS `gatekeep-sync-queue` + DLQ + redrive policy
- [ ] Secrets Manager (`gatekeep/vapid`, `gatekeep/jwks`)
- [ ] IAM roles mínimos para backend y worker

Backend (.NET)
- [ ] Endpoint `POST /api/sync/batch` que encola en SQS (202)
- [ ] Validación JWT/Cognito y limits en tamaño de lote
- [ ] Idempotencia server-side (deviceId + idTemporal)
- [ ] Mappings `idTemporal -> idServidor` persistidos
- [ ] Timestamps `FechaCreacion`/`UltimaActualizacion`
- [ ] Métricas CloudWatch emitidas

Worker
- [ ] Consumidor SQS (Lambda/ECS) con retries y DLQ
- [ ] Persiste EventoAcceso y audit en Mongo
- [ ] Logs estructurados y métricas

Frontend (PWA)
- [ ] `sql-wasm.wasm` en S3 con `Content-Type: application/wasm`
- [ ] `sw.js` y `offline.html` pre-cacheados en CloudFront
- [ ] PWA apunta a dominio CloudFront/ALB para APIs
- [ ] Reintentos exponenciales en `SyncClient` y backoff
- [ ] UX: indicador online/offline y botón manual

Observabilidad & Seguridad
- [ ] Dashboards CloudWatch (SQS depth, 5xx rate, DLQ)
- [ ] Alarms configuradas (SQS depth, 5xx rate, DLQ)
- [ ] WAF básico + CSP + HTTPS obligatorio

---

## 🔧 Operaciones rápidas (comprobaciones y smoke tests)
- Verificar headers del objeto WASM en S3 (Content-Type + CORS).
- Desde staging: abrir app, DevTools → Application → Service Workers → verificar `sw.js` activo.
- Simular offline en DevTools y ejecutar flujo: eventos quedan en SQLite local.
- Reconectar: cliente llama a `/api/sync/batch` y obtiene 202; verificar SQS recibe mensaje.
- Ver logs del worker: mensaje procesado y registro en Postgres y Mongo.

---

## 📈 Observabilidad y alertas (sugerencia mínima)
- Dashboard: SQS Depth, ApproximateAgeOfOldestMessage, /api/sync 5xx rate, Worker processing latency, DLQ messages.
- Alerts: SQS Depth > X (15m), 5xx rate > Y (5m), DLQ messages > 0.

---

## ⚠️ Riesgos y mitigaciones
- R1: WASM no servido correctamente (Content-Type/CORS) -> mitigación: validar headers en S3 y probar carga desde CDN.
- R2: Duplicación de eventos por reintentos -> mitigación: idempotencia (deviceId + idTemporal) y mapping.
- R3: SQS backlog -> mitigación: alarms y worker autoscaling / Lambda concurrency.
- R4: CloudFront sirve offline cuando origen se recuperó -> mitigación: TTL corto en Custom Error Responses e invalidaciones post-deploy.

---

## 📦 Entregables (para equipo)
- Terraform modules listos: `frontend_bucket`, `cloudfront_distribution`, `sqs_queue`, `secrets_manager_entries`, `iam_roles`.
- Documento API: `POST /api/sync/batch` (spec + limits + ejemplos de payload/respuesta).
- Worker package y despliegue (Lambda/ECS) con permisos y pruebas.
- CI/CD pipeline steps: upload S3 + CloudFront invalidation; backend deploy.
- Smoke test checklist automatizable.

---

## 📌 Criterios de aceptación
- WASM y SW cargan desde CDN correctamente en producción
- CloudFront devuelve `offline.html` ante 5xx del origen
- `/api/sync/batch` encola mensajes en SQS y devuelve 202
- Worker procesa mensajes, aplica idempotencia y persiste eventos
- Dashboards y alarms están activos y prueban alerting básico

---

## 🧾 Notas finales
- Priorizar idempotencia y encolado; preferir respuestas 202 para decoupling.
- Mantener listado de assets críticos pre-cacheados en CloudFront.
- Documentar `sync` contract y conflict resolution (server-wins por timestamp por defecto).

---

Fin del plan. Mantengo este archivo como la fuente operativa para los cambios en Terraform y despliegue.
