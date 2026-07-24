import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { DaoneTestComponent } from './daone-test/daone-test.component';
import { SectorsListComponent } from './outdoors/sectors-list/sectors-list.component';
import { sectorsResolver } from './core/resolvers/sector.resolver';
import { SectorComponent } from './outdoors/sector/sector.component';
import { blocResolver, blocsOfSectorResolver } from './core/resolvers/bloc.resolver';
import { SpraywallComponent } from './spraywalls/spraywall/spraywall.component';
import { PrivacyPolicyComponent } from './legal/privacy-policy/privacy-policy.component';
import { TermsComponent } from './legal/terms.component/terms.component';
import { SpraywallEditor } from './spraywalls/spraywall-editor/spraywall-editor';
import { spraywallProblemResolver } from './core/resolvers/spraywall-problem.resolver';
import { OutdoorBloc } from './outdoors/outdoor-bloc/outdoor-bloc';
import { OutdoorEditor } from './outdoors/outdoor-editor/outdoor-editor';
import { lineResolver } from './core/resolvers/line.resolver';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', component: HomeComponent },
  { path: 'privacy-policy', component: PrivacyPolicyComponent },
  { path: 'terms', component: TermsComponent },
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
    path: 'bloc/:id',
    component: OutdoorBloc,
    resolve: {
      bloc: blocResolver
    }
  },
  {
    path: 'bloc-editor/:id/:lineId',
    component: OutdoorEditor,
    resolve: {
      bloc: blocResolver,
      line: lineResolver
    }
  },
  {
    path: 'bloc-editor/:id',
    component: OutdoorEditor,
    resolve: {
      bloc: blocResolver
    }
  },
  {
    path: 'spraywall/:id',
    component: SpraywallComponent
  },
  {
    path: 'spraywall-editor/:spraywallId/:problemId',
    component: SpraywallEditor,
    resolve: {
      spraywallProblem: spraywallProblemResolver
    }
  },
  {
    path: 'spraywall-editor/:spraywallId',
    component: SpraywallEditor
  }
];
