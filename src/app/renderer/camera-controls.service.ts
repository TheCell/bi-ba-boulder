import { Injectable, signal } from '@angular/core';
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';

@Injectable({
  providedIn: 'root'
})
export class CameraControlsService {
  public isDefaultPosition = signal<boolean>(false);

  private orbitControls: OrbitControls | undefined;
  private defaultPosition: THREE.Vector3 | undefined;
  private defaultDistance = 0;
  private thresholdDistance = 0.005;

  public setOrbitControls(orbitControls: OrbitControls): void {
    this.orbitControls = orbitControls;
    this.isDefaultPosition.set(true);
    this.defaultDistance = 0;
    this.orbitControls.update();
    this.defaultPosition = new THREE.Vector3(
      this.orbitControls.getDistance(),
      this.orbitControls.getAzimuthalAngle(),
      this.orbitControls.getPolarAngle()
    );
    this.orbitControls.saveState();

    this.orbitControls.addEventListener('change', () => {
      if (this.orbitControls && this.defaultPosition) {
        const currentPosition = new THREE.Vector3(
          this.orbitControls.getDistance(),
          this.orbitControls.getAzimuthalAngle(),
          this.orbitControls.getPolarAngle()
        );
        const distance = currentPosition.distanceTo(this.defaultPosition);
        if (
          distance > this.defaultDistance + this.thresholdDistance ||
          distance < this.defaultDistance - this.thresholdDistance
        ) {
          this.isDefaultPosition.set(false);
        } else {
          this.isDefaultPosition.set(true);
        }
      } else {
        console.warn('OrbitControls or default position not set. Cannot determine if camera is in default position.');
      }
    });
  }

  public resetCameraPosition(): void {
    if (this.orbitControls) {
      this.orbitControls.reset();
    } else {
      console.warn('OrbitControls not set. Cannot reset camera position.');
    }
  }
}
