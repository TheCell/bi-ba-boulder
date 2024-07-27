import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CanvasBoxComponent } from './components/canvas-box/canvas-box.component';
import { BoulderRenderComponent } from './boulder-render/boulder-render.component';

export const routes: Routes = [
  { path: 'home', component: HomeComponent },
  { path: 'canvas-box', component: CanvasBoxComponent },
  { path: 'boulder-render', component: BoulderRenderComponent },
];
