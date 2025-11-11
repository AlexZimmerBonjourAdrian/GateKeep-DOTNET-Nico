# Demo1.Api — Minimal API en .NET 8 (Strategy + Decorator + Factory + Template Method)

Autor: **Nico Escobar**

## 🔎 Descripción
API mínima para cálculo de precios por país. Demuestra:
- **Strategy**: impuestos por país.
- **Decorator**: descuento opcional desde `appsettings.json`.
- **Factory**: compone el cálculo (Strategy [+ Decorator]).
- **Template Method**: formato del recibo.
- **IMemoryCache**: cachea estrategias por país (exp. deslizante 30 min).

## 🧰 Requisitos
- .NET SDK **8.x**
- Kestrel (servidor por defecto)
- Swagger/OpenAPI habilitado

**Base URL (por defecto):** `http://localhost:5011`  
**Swagger UI:** `http://localhost:5011/swagger`

## ▶️ Ejecución

```bash
# macOS / Linux
./scripts/run.sh

# Windows (PowerShell)
./scripts/run.ps1
```

> Los scripts deben establecer `ASPNETCORE_URLS=http://localhost:5011` o equivalente.

## ⚙️ Configuración

Archivo `appsettings.json` (fragmento relevante):

```json
{
  "Pricing": {
    "DiscountRate": 0.05
  }
}
```

- **Rango aceptado:** `0.0 .. 1.0` (5% ⇒ `0.05`).
- Si falta o es `0`, no se aplica descuento (no se usa el Decorator).

## 🌐 Endpoints

| Método | Ruta                                   | Descripción                                 | Códigos |
|-------:|----------------------------------------|---------------------------------------------|:-------:|
| POST   | `/countries`                           | Crea/actualiza un país en memoria.          | 201     |
| GET    | `/countries/{id}`                      | Obtiene un país por id.                     | 200/404 |
| GET    | `/countries`                           | Lista todos los países.                     | 200     |
| DELETE | `/countries`                           | Elimina todos los países (en memoria).      | 204     |
| GET    | `/pricing/{country}/{amount}`          | Calcula precio (base + impuestos [+ desc]). | 200/400/404 |
| GET    | `/health` *(opcional)*                 | Estado básico del servicio.                  | 200     |

### Ejemplos

**Crear/actualizar país**

```http
POST /countries HTTP/1.1
Host: localhost:5011
Content-Type: application/json

{
  "id": "UY",
  "name": "Uruguay",
  "currency": "UYU",
  "taxRate": 0.22
}
```

**Respuesta**
```
201 Created
Location: /countries/UY
```

**Obtener país**
```http
GET /countries/UY HTTP/1.1
Host: localhost:5011
```

**Listar países**
```http
GET /countries HTTP/1.1
Host: localhost:5011
```

**Calcular precio**
```http
GET /pricing/UY/100 HTTP/1.1
Host: localhost:5011
```

**Respuesta (200 OK)**
```json
{
  "country": "UY",
  "baseAmount": 100,
  "finalAmount": 115.9,
  "receipt": "== Retail Receipt ==\nBase: 100, Final: 115.9\nGenerated at 2025-08-24 12:34:56Z"
}
```

**cURL**
```bash
curl -X POST "http://localhost:5011/countries"   -H "Content-Type: application/json"   -d '{"id":"UY","name":"Uruguay","currency":"UYU","taxRate":0.22}'

curl "http://localhost:5011/pricing/UY/100"
```

### Errores comunes
| Código | Motivo                                     |
|:-----:|---------------------------------------------|
| 400   | Estrategia no soportada / parámetros inválidos. |
| 404   | País no registrado.                         |

## 🧩 Patrones (resumen)
- **Strategy** (`ITaxStrategy`, `ConfigurableTaxStrategy`): define el impuesto por país (usa `taxRate` guardado al registrar).
- **Decorator** (`DiscountDecorator`): aplica opcionalmente un descuento global `Pricing:DiscountRate`.
- **Factory** (`PriceCalculatorFactory`): arma el cálculo (Base + Decorator si corresponde) y usa `IMemoryCache` por país.
- **Template Method** (`ReceiptGenerator` → `RetailReceiptGenerator`): compone `Header → Body → Footer` del recibo.

## 📦 Contratos (DTOs principales)
- `Country`: `{ id, name, currency, taxRate }`
- `PricingResponse`: `{ country, baseAmount, finalAmount, receipt }`

## 📝 Notas
- **Persistencia:** almacenamiento de `Country` es en memoria (se pierde al reiniciar).
- Cambiá `DiscountRate` y **reiniciá** para ver el efecto en `finalAmount`.
- La raíz `/` redirige a `/swagger` y puede excluirse de la doc si así se configuró.
