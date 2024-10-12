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

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', component: HomeComponent },
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
];
