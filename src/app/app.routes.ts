import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { SectorsListComponent } from './outdoors/sectors-list/sectors-list.component';
import { sectorResolver, sectorsResolver } from './core/resolvers/sector.resolver';
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
import { ChangelogComponent } from './changelog/changelog.component';
import { OutdoorAreaOverview } from './outdoors/outdoor-area-overview/outdoor-area-overview';
import { outdoorAreaResolver } from './core/resolvers/outdoor-area.resolver';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', component: HomeComponent },
  { path: 'privacy-policy', component: PrivacyPolicyComponent },
  { path: 'terms', component: TermsComponent },
  { path: 'changelog', component: ChangelogComponent },
  {
    path: 'sectors',
    pathMatch: 'full',
    component: SectorsListComponent,
    resolve: {
      sectors: sectorsResolver
    }
  },
  {
    path: 'outdoor-area/:outdoorAreaId',
    component: OutdoorAreaOverview,
    resolve: {
      outdoorArea: outdoorAreaResolver
    }
  },
  {
    path: 'sectors/:sectorId',
    component: SectorComponent,
    resolve: {
      blocs: blocsOfSectorResolver,
      sector: sectorResolver
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
