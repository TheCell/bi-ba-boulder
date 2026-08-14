import { Route } from '@angular/router';
import { routes } from './app.routes';

describe('outdoor routes', (): void => {
  it('keeps direct bloc links available', (): void => {
    const directBlocRoute: Route | undefined = routes.find((route: Route): boolean => route.path === 'bloc/:id');

    expect(directBlocRoute).toBeDefined();
  });

  it('provides the complete outdoor hierarchy for contextual bloc links', (): void => {
    const contextualBlocRoute: Route | undefined = routes.find(
      (route: Route): boolean => route.path === 'outdoor-area/:outdoorAreaId/sector/:sectorId/bloc/:id'
    );

    expect(contextualBlocRoute).toBeDefined();
    expect(contextualBlocRoute?.resolve?.['outdoorArea']).toBeDefined();
    expect(contextualBlocRoute?.resolve?.['sector']).toBeDefined();
    expect(contextualBlocRoute?.resolve?.['blocs']).toBeDefined();
    expect(contextualBlocRoute?.resolve?.['bloc']).toBeDefined();
  });
});
