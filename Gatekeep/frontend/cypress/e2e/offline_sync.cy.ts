describe('Offline sync experience', () => {
  beforeEach(() => {
    cy.visit('/');
    cy.get('[data-cy="sync-badge"]', { timeout: 15000 }).should('exist');
  });

  it('muestra el indicador de sincronización online', () => {
    cy.get('[data-cy="sync-badge"]').should('contain.text', 'Online');
  });

  it('cambia a estado offline cuando el navegador pierde conexión', () => {
    cy.window().then((win) => {
      Object.defineProperty(win.navigator, 'onLine', {
        configurable: true,
        value: false,
      });
      win.dispatchEvent(new win.Event('offline'));
    });

    cy.get('[data-cy="sync-badge"]')
      .should('contain.text', 'Offline')
      .click();

    cy.get('.sync-details').should('exist');
  });
});

