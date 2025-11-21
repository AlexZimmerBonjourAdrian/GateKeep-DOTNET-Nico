/// <reference types="cypress" />

// Support file para Cypress E2E tests
// Importar comandos personalizados y configuración global

// Type definitions
declare global {
  namespace Cypress {
    interface Chainable {
      setOffline(): Chainable<void>;
      setOnline(): Chainable<void>;
      mockSyncAPI(response: any): Chainable<void>;
    }
  }
}

// Custom command para simular offline
(Cypress.Commands as any).add('setOffline', () => {
  cy.window().then((win: any) => {
    Object.defineProperty(win.navigator, 'onLine', {
      writable: true,
      value: false,
      configurable: true,
    });
    win.dispatchEvent(new Event('offline'));
  });
});

// Custom command para simular online
(Cypress.Commands as any).add('setOnline', () => {
  cy.window().then((win: any) => {
    Object.defineProperty(win.navigator, 'onLine', {
      writable: true,
      value: true,
      configurable: true,
    });
    win.dispatchEvent(new Event('online'));
  });
});

// Custom command para mock de API
(Cypress.Commands as any).add('mockSyncAPI', (response: any) => {
  cy.intercept('POST', '**/api/sync', {
    statusCode: 200,
    body: response,
  }).as('sync');
});

// Manejo de excepciones
(Cypress as any).on('uncaught:exception', (err: Error) => {
  // Ignorar errores específicos si es necesario
  if (err.message.includes('ResizeObserver')) {
    return false;
  }
  return true;
});

export {};
