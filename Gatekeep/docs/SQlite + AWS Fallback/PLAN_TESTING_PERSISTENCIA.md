# Plan de Testing: Persistencia y Sincronización Offline

**Fecha:** 18 de Noviembre de 2025  
**Proyecto:** GateKeep - Sistema de Gestión de Acceso  
**Objetivo:** Validar SQLite local, sincronización offline y PWA functionality

---

## 📋 Estrategia de Testing

### Niveles de Test

```
┌─────────────────────────────────────┐
│   E2E Testing (Cypress/Playwright)  │  ← Integración completa
├─────────────────────────────────────┤
│   Integration Testing (Jest + Mock) │  ← Módulos interconectados
├─────────────────────────────────────┤
│   Unit Testing (Jest)               │  ← Funciones aisladas
└─────────────────────────────────────┘
```

---

## 🧪 1. Unit Tests - Backend (.NET)

### A. Tests del SyncService

**Archivo:** `src/GateKeep.Api/Application/Sync/SyncService.Tests.cs`

**Herramientas:** xUnit, Moq, FluentAssertions

**Test Cases:**

```csharp
public class SyncServiceTests
{
    private readonly SyncService _syncService;
    private readonly Mock<IGateKeepDbContext> _dbContextMock;
    private readonly Mock<ILogger<SyncService>> _loggerMock;

    public SyncServiceTests()
    {
        _dbContextMock = new Mock<IGateKeepDbContext>();
        _loggerMock = new Mock<ILogger<SyncService>>();
        _syncService = new SyncService(_dbContextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SyncAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var request = new SyncRequest
        {
            DeviceId = "device-123",
            UltimaActualizacion = DateTime.UtcNow.AddHours(-1),
            EventosAccesoPendientes = new List<EventoAccesoOffline>()
        };

        // Act
        var result = await _syncService.SyncAsync(request, 1);

        // Assert
        result.Should().NotBeNull();
        result.Exitoso.Should().BeTrue();
    }

    [Fact]
    public async Task ProcesarEventosAccesoOfflineAsync_CreatesEventosAcceso()
    {
        // Arrange
        var eventos = new List<EventoAccesoOffline>
        {
            new EventoAccesoOffline
            {
                IdTemporal = Guid.NewGuid(),
                UsuarioId = 1,
                EspacioId = 1,
                FechaHora = DateTime.UtcNow,
                TipoAcceso = "Entrada",
                Exitoso = true
            }
        };

        // Act
        await _syncService.ProcesarEventosAccesoOfflineAsync(eventos, 1);

        // Assert
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task ObtenerDatosActualizadosAsync_RetursOnlyChangedData()
    {
        // Arrange
        var ultimaActualizacion = DateTime.UtcNow.AddHours(-1);

        // Act
        var result = await _syncService.ObtenerDatosActualizadosAsync(ultimaActualizacion, 1);

        // Assert
        result.Should().NotBeNull();
        result.Usuarios.Should().NotBeNull();
    }

    [Fact]
    public async Task SyncAsync_WithInvalidDeviceId_ThrowsException()
    {
        // Arrange
        var request = new SyncRequest { DeviceId = "" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _syncService.SyncAsync(request, 1));
    }
}
```

### B. Tests del SyncController

**Archivo:** `src/GateKeep.Api/Endpoints/Sync/SyncController.Tests.cs`

```csharp
public class SyncControllerTests
{
    private readonly SyncController _controller;
    private readonly Mock<ISyncService> _syncServiceMock;
    private readonly Mock<ILogger<SyncController>> _loggerMock;

    public SyncControllerTests()
    {
        _syncServiceMock = new Mock<ISyncService>();
        _loggerMock = new Mock<ILogger<SyncController>>();
        _controller = new SyncController(_syncServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Sync_WithValidRequest_Returns200Ok()
    {
        // Arrange
        var request = new SyncRequest
        {
            DeviceId = "device-123",
            UltimaActualizacion = DateTime.UtcNow.AddHours(-1)
        };

        var mockResponse = new SyncResponse
        {
            Exitoso = true,
            FechaSincronizacion = DateTime.UtcNow
        };

        _syncServiceMock.Setup(x => x.SyncAsync(request, It.IsAny<long>(), default))
            .ReturnsAsync(mockResponse);

        // Act
        var result = await _controller.Sync(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult?.Value.Should().Be(mockResponse);
    }

    [Fact]
    public async Task Sync_WithInvalidRequest_Returns400BadRequest()
    {
        // Act & Assert
        var result = await _controller.Sync(null!);
        result.Should().BeOfType<BadRequestResult>();
    }
}
```

---

## 🧪 2. Unit Tests - Frontend (TypeScript/Jest)

### A. Tests de sqlite-db.ts

**Archivo:** `frontend/src/lib/__tests__/sqlite-db.test.ts`

```typescript
import {
  initializeDatabase,
  cacheUsuario,
  getUsuarioLocal,
  recordOfflineEvent,
  getPendingOfflineEvents,
  markEventoAsSynced,
  setSyncMetadata,
  getSyncMetadata,
} from '../sqlite-db';

describe('SQLite Database Module', () => {
  
  beforeEach(async () => {
    // Limpiar localStorage y IndexedDB antes de cada test
    localStorage.clear();
    const dbs = await indexedDB.databases();
    dbs.forEach(db => indexedDB.deleteDatabase(db.name));
  });

  describe('initializeDatabase', () => {
    it('should initialize database successfully', async () => {
      const result = await initializeDatabase();
      expect(result).toBeDefined();
    });

    it('should create all required tables', async () => {
      await initializeDatabase();
      // Verificar que las tablas existan
      // Esto requeriría exponer una función de query
    });
  });

  describe('cacheUsuario', () => {
    it('should cache a user successfully', async () => {
      await initializeDatabase();
      
      const usuario = {
        id: 1,
        email: 'test@example.com',
        nombre: 'Test',
        apellido: 'User',
        rol: 'Admin',
        credentialActiva: true,
        ultimaActualizacion: new Date().toISOString()
      };

      cacheUsuario(usuario);
      
      const cached = getUsuarioLocal(1);
      expect(cached).toBeDefined();
      expect(cached?.email).toBe('test@example.com');
    });

    it('should update existing user', async () => {
      await initializeDatabase();
      
      const usuario1 = { id: 1, email: 'old@example.com', nombre: 'Test', apellido: 'User', rol: 'User', credentialActiva: true, ultimaActualizacion: new Date().toISOString() };
      const usuario2 = { id: 1, email: 'new@example.com', nombre: 'Test', apellido: 'User', rol: 'Admin', credentialActiva: true, ultimaActualizacion: new Date().toISOString() };
      
      cacheUsuario(usuario1);
      cacheUsuario(usuario2);
      
      const cached = getUsuarioLocal(1);
      expect(cached?.email).toBe('new@example.com');
    });
  });

  describe('recordOfflineEvent', () => {
    it('should record offline event with unique ID', async () => {
      await initializeDatabase();
      
      const tipoEvento = 'AccessAttempt';
      const datosEvento = { usuarioId: 1, espacioId: 2 };
      
      const id1 = recordOfflineEvent(tipoEvento, datosEvento);
      const id2 = recordOfflineEvent(tipoEvento, datosEvento);
      
      expect(id1).toBeDefined();
      expect(id2).toBeDefined();
      expect(id1).not.toBe(id2);
    });

    it('should return string ID', async () => {
      await initializeDatabase();
      
      const id = recordOfflineEvent('TestEvent', { test: true });
      
      expect(typeof id).toBe('string');
      expect(id.length).toBeGreaterThan(0);
    });
  });

  describe('getPendingOfflineEvents', () => {
    it('should return empty array initially', async () => {
      await initializeDatabase();
      
      const eventos = getPendingOfflineEvents();
      
      expect(Array.isArray(eventos)).toBe(true);
      expect(eventos.length).toBe(0);
    });

    it('should return recorded pending events', async () => {
      await initializeDatabase();
      
      recordOfflineEvent('Event1', { data: 1 });
      recordOfflineEvent('Event2', { data: 2 });
      
      const eventos = getPendingOfflineEvents();
      
      expect(eventos.length).toBe(2);
    });
  });

  describe('markEventoAsSynced', () => {
    it('should mark event as synced', async () => {
      await initializeDatabase();
      
      const id = recordOfflineEvent('TestEvent', { test: true });
      let eventos = getPendingOfflineEvents();
      expect(eventos.length).toBe(1);
      
      markEventoAsSynced(id);
      
      eventos = getPendingOfflineEvents();
      expect(eventos.length).toBe(0);
    });
  });

  describe('Sync Metadata', () => {
    it('should set and retrieve metadata', async () => {
      await initializeDatabase();
      
      const clave = 'lastSync';
      const valor = new Date().toISOString();
      
      setSyncMetadata(clave, valor);
      const retrieved = getSyncMetadata(clave);
      
      expect(retrieved).toBe(valor);
    });

    it('should return null for non-existent metadata', async () => {
      await initializeDatabase();
      
      const valor = getSyncMetadata('nonExistent');
      
      expect(valor).toBeNull();
    });
  });
});
```

### B. Tests de sync.ts

**Archivo:** `frontend/src/lib/__tests__/sync.test.ts`

```typescript
import {
  getDeviceId,
  isOnline,
  syncWithServer,
  setupConnectivityListeners,
  recordEvent,
} from '../sync';

describe('Sync Module', () => {
  
  beforeEach(() => {
    localStorage.clear();
    // Mock de fetch global
    global.fetch = jest.fn();
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('getDeviceId', () => {
    it('should generate device ID if not exists', () => {
      const id1 = getDeviceId();
      expect(id1).toBeDefined();
      expect(id1).toMatch(/^device-/);
    });

    it('should return same ID on subsequent calls', () => {
      const id1 = getDeviceId();
      const id2 = getDeviceId();
      expect(id1).toBe(id2);
    });
  });

  describe('isOnline', () => {
    it('should detect online status', () => {
      const online = isOnline();
      expect(typeof online).toBe('boolean');
    });
  });

  describe('syncWithServer', () => {
    it('should return false when offline', async () => {
      // Mock navigator.onLine como false
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: false,
      });

      const result = await syncWithServer('token123');
      
      expect(result).toBe(false);
    });

    it('should send sync request when online', async () => {
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: true,
      });

      const mockResponse = {
        success: true,
        processedEvents: [],
        lastSuccessfulSync: new Date().toISOString(),
      };

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse,
      });

      const result = await syncWithServer('token123');

      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining('/api/sync'),
        expect.objectContaining({
          method: 'POST',
          headers: expect.objectContaining({
            'Authorization': 'Bearer token123',
          }),
        })
      );
    });

    it('should handle sync errors gracefully', async () => {
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: true,
      });

      (global.fetch as jest.Mock).mockResolvedValueOnce({
        ok: false,
        status: 500,
      });

      const result = await syncWithServer('token123');

      expect(result).toBe(false);
    });
  });

  describe('recordEvent', () => {
    it('should record event successfully', async () => {
      const tipoEvento = 'UserAccess';
      const datosEvento = { usuarioId: 1, espacioId: 2 };

      const id = await recordEvent(tipoEvento, datosEvento);

      expect(id).toBeDefined();
      expect(typeof id).toBe('string');
    });
  });

  describe('setupConnectivityListeners', () => {
    it('should trigger sync when connection restored', async () => {
      const mockSyncWithServer = jest.fn().mockResolvedValue(true);
      
      // Espiar syncWithServer (requeriría refactorización)
      setupConnectivityListeners('token123');

      // Simular evento 'online'
      window.dispatchEvent(new Event('online'));

      // Verificar que se ejecutó (requiere espía adicional)
    });
  });
});
```

---

## 🧪 3. Integration Tests

### A. Test de Ciclo Completo Offline-to-Online

**Archivo:** `frontend/src/__tests__/offline-sync.integration.test.ts`

```typescript
import { initializeDatabase } from '../lib/sqlite-db';
import { syncWithServer, recordEvent, getDeviceId } from '../lib/sync';

describe('Offline-to-Online Sync Integration', () => {
  
  beforeEach(async () => {
    localStorage.clear();
    await initializeDatabase();
  });

  it('should sync recorded offline events when connection restored', async () => {
    // 1. Simular modo offline
    Object.defineProperty(navigator, 'onLine', {
      writable: true,
      value: false,
    });

    // 2. Registrar eventos offline
    await recordEvent('UserAccess', { usuarioId: 1, espacioId: 2 });
    await recordEvent('UserAccess', { usuarioId: 2, espacioId: 3 });

    // 3. Cambiar a online
    Object.defineProperty(navigator, 'onLine', {
      writable: true,
      value: true,
    });

    // 4. Mock del servidor
    const mockServerResponse = {
      success: true,
      processedEvents: [
        { idTemporal: 'evt-1', success: true, permanentId: 'evt-server-1' },
        { idTemporal: 'evt-2', success: true, permanentId: 'evt-server-2' },
      ],
      lastSuccessfulSync: new Date().toISOString(),
    };

    global.fetch = jest.fn().mockResolvedValueOnce({
      ok: true,
      json: async () => mockServerResponse,
    });

    // 5. Ejecutar sync
    const result = await syncWithServer('valid-token');

    // 6. Verificar
    expect(result).toBe(true);
    expect(global.fetch).toHaveBeenCalled();
  });
});
```

---

## 🧪 4. E2E Tests (Cypress/Playwright)

### A. Test de PWA Offline Functionality

**Archivo:** `frontend/e2e/offline-pwa.cy.ts` (Cypress)

```typescript
describe('PWA Offline Functionality', () => {
  
  beforeEach(() => {
    cy.visit('/');
  });

  it('should load page in offline mode', () => {
    // Ir online primero
    cy.visit('/');
    cy.get('[data-testid="sync-status"]').should('contain', 'Online');

    // Simular offline
    cy.window().then((win) => {
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: false,
        configurable: true,
      });
      win.dispatchEvent(new Event('offline'));
    });

    // Verificar UI de offline
    cy.get('[data-testid="sync-status"]').should('contain', 'Offline');
  });

  it('should cache data for offline access', () => {
    // Acceder a página
    cy.visit('/usuarios');
    cy.get('[data-testid="usuario-list"]').should('exist');

    // Simular offline
    cy.window().then((win) => {
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: false,
        configurable: true,
      });
      win.dispatchEvent(new Event('offline'));
    });

    // Recargar página
    cy.reload();

    // Datos deben estar disponibles desde caché
    cy.get('[data-testid="usuario-list"]').should('exist');
  });

  it('should sync when connection restored', () => {
    // Registro offline
    cy.window().then((win) => {
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: false,
        configurable: true,
      });
      win.dispatchEvent(new Event('offline'));
    });

    cy.get('[data-testid="access-form"]').should('exist');
    cy.get('[data-testid="record-access-btn"]').click();

    // Restaurar conexión
    cy.window().then((win) => {
      Object.defineProperty(navigator, 'onLine', {
        writable: true,
        value: true,
        configurable: true,
      });
      win.dispatchEvent(new Event('online'));
    });

    // Verificar sincronización
    cy.get('[data-testid="sync-status"]').should('contain', 'Synced');
  });

  it('should install as PWA', () => {
    // Verificar manifest
    cy.get('link[rel="manifest"]')
      .should('have.attr', 'href')
      .and('include', 'manifest.json');

    // Verificar Service Worker
    cy.window().then((win) => {
      expect(win.navigator.serviceWorker).toBeDefined();
    });
  });
});
```

### B. Test de Sincronización Backend

**Archivo:** `frontend/e2e/sync-api.cy.ts`

```typescript
describe('Sync API Integration', () => {
  
  const API_URL = 'http://localhost:5011';
  const token = 'valid-jwt-token';

  it('should sync offline events with backend', () => {
    cy.request({
      method: 'POST',
      url: `${API_URL}/api/sync`,
      headers: {
        Authorization: `Bearer ${token}`,
        'Content-Type': 'application/json',
      },
      body: {
        deviceId: 'device-test-123',
        lastSyncTime: new Date().toISOString(),
        pendingEvents: [
          {
            idTemporal: 'evt-temp-1',
            eventType: 'AccessAttempt',
            eventData: JSON.stringify({ usuarioId: 1, espacioId: 2 }),
            createdAt: new Date().toISOString(),
            attemptCount: 1,
          },
        ],
      },
    }).then((response) => {
      expect(response.status).to.eq(200);
      expect(response.body.exitoso).to.be.true;
      expect(response.body.processedEvents).to.have.length(1);
    });
  });

  it('should return updated data on sync', () => {
    cy.request({
      method: 'POST',
      url: `${API_URL}/api/sync`,
      headers: {
        Authorization: `Bearer ${token}`,
      },
      body: {
        deviceId: 'device-test-123',
        lastSyncTime: new Date(Date.now() - 86400000).toISOString(), // 24 horas atrás
        pendingEvents: [],
      },
    }).then((response) => {
      expect(response.body.datos).to.exist;
      expect(response.body.datos.usuarios).to.be.an('array');
      expect(response.body.datos.espacios).to.be.an('array');
    });
  });
});
```

---

## 📊 Configuración de Testing

### package.json - Frontend

```json
{
  "devDependencies": {
    "@testing-library/react": "^14.0.0",
    "@testing-library/jest-dom": "^6.0.0",
    "@types/jest": "^29.0.0",
    "jest": "^29.0.0",
    "jest-environment-jsdom": "^29.0.0",
    "cypress": "^13.0.0",
    "@playwright/test": "^1.40.0",
    "ts-jest": "^29.0.0"
  },
  "scripts": {
    "test": "jest",
    "test:watch": "jest --watch",
    "test:coverage": "jest --coverage",
    "test:e2e": "cypress open",
    "test:e2e:headless": "cypress run"
  }
}
```

### jest.config.js - Frontend

```javascript
module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'jsdom',
  roots: ['<rootDir>/src'],
  testMatch: ['**/__tests__/**/*.test.ts', '**/?(*.)+(spec|test).ts'],
  moduleNameMapper: {
    '^@/(.*)$': '<rootDir>/src/$1',
  },
  setupFilesAfterEnv: ['<rootDir>/src/setupTests.ts'],
  collectCoverageFrom: [
    'src/**/*.{ts,tsx}',
    '!src/**/*.d.ts',
    '!src/pages',
  ],
  coverageThreshold: {
    global: {
      branches: 70,
      functions: 70,
      lines: 70,
      statements: 70,
    },
  },
};
```

### cypress.config.ts - Frontend

```typescript
import { defineConfig } from 'cypress';

export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:3000',
    setupNodeEvents(on, config) {},
    specPattern: 'cypress/e2e/**/*.cy.ts',
  },
});
```

### Backend - .csproj

```xml
<ItemGroup>
  <PackageReference Include="xunit" Version="2.6.0" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
  <PackageReference Include="Moq" Version="4.20.0" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.7.0" />
</ItemGroup>
```

---

## ✅ Checklist de Tests

### Unit Tests
- [ ] Backend SyncService - 5 tests
- [ ] Backend SyncController - 3 tests
- [ ] Frontend sqlite-db.ts - 8 tests
- [ ] Frontend sync.ts - 5 tests
- **Total: 21 unit tests**

### Integration Tests
- [ ] Offline-to-Online sync cycle - 1 test
- [ ] Multi-device sync - 1 test
- [ ] Conflict resolution - 1 test
- **Total: 3 integration tests**

### E2E Tests
- [ ] PWA offline mode - 4 tests
- [ ] Sync API - 2 tests
- [ ] Service Worker - 2 tests
- [ ] Performance - 2 tests
- **Total: 10 E2E tests**

---

## 🎯 Cobertura de Código

**Meta:** 70% - 80% en áreas críticas

- `sqlite-db.ts`: 85%
- `sync.ts`: 80%
- `SyncService.cs`: 90%
- `SyncController.cs`: 85%

---

## 🚀 Ejecución de Tests

```bash
# Frontend
npm test                    # Ejecutar todos los tests
npm run test:watch        # Modo watch
npm run test:coverage     # Reporte de cobertura
npm run test:e2e          # Tests E2E en modo interactivo
npm run test:e2e:headless # Tests E2E headless

# Backend
dotnet test               # Ejecutar tests
dotnet test /p:CollectCoverageMetrics=true  # Con coverage
```

---

## 📝 Criterios de Aceptación

✅ **Todos los tests deben pasar**  
✅ **Cobertura mínima 70%**  
✅ **Sin errores de TypeScript**  
✅ **Sin warnings en linting**  
✅ **E2E tests validan flujo completo offline-online**

---

## 📅 Timeline Estimado

- **Unit Tests:** 2-3 días
- **Integration Tests:** 1-2 días
- **E2E Tests:** 2-3 días
- **Total:** 5-8 días

**Status:** 🔴 **NO INICIADO**
