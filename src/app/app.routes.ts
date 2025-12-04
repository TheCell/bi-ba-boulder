import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CanvasBoxComponent } from './components/canvas-box/canvas-box.component';
import { BoulderHardcodedComponent } from './boulder-hardcoded/boulder-hardcoded.component';
import { DaoneTestComponent } from './daone-test/daone-test.component';
import { SectorsListComponent } from './sectors-list/sectors-list.component';
import { sectorsResolver } from './core/resolvers/sector.resolver';
import { SectorComponent } from './sector/sector.component';
import { blocResolver, blocsOfSectorResolver } from './core/resolvers/bloc.resolver';
import { BoulderComponent } from './boulder/boulder.component';
import { SpraywallTestComponent } from './spraywall-test/spraywall-test.component';
import { SpraywallTest2Component } from './spraywall-test-2/spraywall-test-2.component';
import { SpraywallComponent } from './spraywall/spraywall.component';
import { PrivacyPolicyComponent } from './legal/privacy-policy/privacy-policy.component';
import { TermsComponent } from './legal/terms.component/terms.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', component: HomeComponent },
  { path: 'privacy-policy', component: PrivacyPolicyComponent },
  { path: 'terms', component: TermsComponent },
  { path: 'canvas-box', component: CanvasBoxComponent },
  { path: 'single-boulder-test', component: BoulderHardcodedComponent },
  {
    path: 'daone-boulder-test',
    component: DaoneTestComponent,
    children: [
      { path: '', component: DaoneTestComponent, pathMatch: 'full' },
      { path: ':number', component: DaoneTestComponent }
    ]
   },
   {
    path: 'sectors',
    pathMatch: 'full',
    component: SectorsListComponent,
    resolve: {
      sectors: sectorsResolver
    }
  },
  {
    path: 'sectors/:sectorId',
    component: SectorComponent,
    resolve: {
      blocs: blocsOfSectorResolver
    }
  },
  {
    path: 'boulder/:id',
    component: BoulderComponent,
    resolve: {
      bloc: blocResolver
    }
  },
  {
    path: 'spraywall/:id',
    component: SpraywallComponent
  },
  {
    path: 'spraywall-test',
    component: SpraywallTestComponent
  },
  {
    path: 'spraywall-test-2',
    component: SpraywallTest2Component
  }
];
