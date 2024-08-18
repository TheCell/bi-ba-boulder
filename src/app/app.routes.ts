import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CanvasBoxComponent } from './components/canvas-box/canvas-box.component';
import { BoulderRenderComponent } from './boulder-render/boulder-render.component';
import { BoulderComponent } from './boulder/boulder.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'home' },
  { path: 'home', component: HomeComponent },
  { path: 'canvas-box', component: CanvasBoxComponent },
  { path: 'boulder-render', component: BoulderRenderComponent },
  { path: 'single-boulder-test', component: BoulderComponent },
];
