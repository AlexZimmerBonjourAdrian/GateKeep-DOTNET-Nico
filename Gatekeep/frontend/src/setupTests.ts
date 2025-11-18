/**
 * Setup file para Jest
 * Configuración global para todos los tests
 */

import '@testing-library/jest-dom';

declare const jest: any;

// Mock de IndexedDB
class MockIndexedDB {
  databases = jest.fn().mockResolvedValue([]);
  open = jest.fn().mockReturnValue({
    onsuccess: undefined,
    onerror: undefined,
    onupgradeneeded: undefined,
  });
  deleteDatabase = jest.fn();
}

// Mock de localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};

// Asignar mocks globales
Object.defineProperty(window, 'indexedDB', {
  value: new MockIndexedDB(),
  writable: true,
});

Object.defineProperty(window, 'localStorage', {
  value: localStorageMock,
});

// Mock de navigator.onLine
Object.defineProperty(navigator, 'onLine', {
  writable: true,
  value: true,
});

// Suprimir logs en tests
global.console = {
  ...console,
  log: jest.fn(),
  debug: jest.fn(),
  info: jest.fn(),
  warn: jest.fn(),
  error: jest.fn(),
};
