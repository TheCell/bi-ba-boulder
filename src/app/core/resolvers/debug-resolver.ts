import { inject } from '@angular/core';
import type { ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot } from '@angular/router';
import { DebugLoader } from '../../background-loading/debug-loader';

export const spraywallDebugTextureResolver: ResolveFn<ArrayBuffer> = (_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => inject(DebugLoader).loadSpraywallDebugTexture();
