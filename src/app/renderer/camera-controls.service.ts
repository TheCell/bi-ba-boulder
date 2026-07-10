import { Injectable, signal } from '@angular/core';
import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
import { Viewpoint } from './common/viewpoint';

@Injectable({
  providedIn: 'root'
})
export class CameraControlsService {
  public isDefaultPosition = signal<boolean>(false);

  private orbitControls: OrbitControls | undefined;
  private camera: THREE.Camera | undefined;
  private defaultPosition: THREE.Vector3 | undefined;
  private defaultDistance = 0;
  private thresholdDistance = 0.005;

  public setOrbitControls(orbitControls: OrbitControls): void {
    this.orbitControls = orbitControls;
    this.camera = orbitControls.object;
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

  /**
   * This method is intended to animate the camera transition to a new viewpoint.
   * This method should be called in the animation loop to ensure smooth transitions.
   */
  public animateTransition(): void {
    if (!this.needsAnimation || !this.camera || !this.orbitControls) {
      return;
    }

    console.log('animateTransition()');

    const currentTime = performance.now();
    const elapsedTime = currentTime - this.transitionStartTime;

    const t = Math.min(elapsedTime / this.transitionDurationInMs, 1); // Clamp t to [0, 1]
    // const easedT = t * t * (3 - 2 * t);
    const easedT = t;
    console.log(easedT);

    this.camera.position.lerpVectors(this.camera.position, this.transitionTargetPosition, easedT);
    this.orbitControls.target.lerpVectors(this.orbitControls.target, this.transitionTargetPosition, easedT);
    this.orbitControls.update();

    if (easedT >= 1) {
      this.camera.position.copy(this.transitionTargetPosition);
      this.orbitControls.target.copy(this.transitionTargetPosition);
      this.orbitControls.update();
      this.transitionTargetPosition.set(0, 0, 0);
      this.needsAnimation = false;
      // this.orbitControls.enabled = true;
    }
  }

  public goToView(viewpoint: Viewpoint): void {
    if (!this.camera || !this.orbitControls) {
      console.warn('Camera or OrbitControls not set. Cannot go to view.');
      return;
    }

    this.setViewTransitionAnimation(viewpoint);
  }

  public needsAnimation = false;
  private transitionTargetPosition = new THREE.Vector3();
  private transitionStartTime = 0;
  private transitionDurationInMs = 1000;
  private setViewTransitionAnimation(viewpoint: Viewpoint): void {
    this.transitionTargetPosition.copy(viewpoint.target);
    this.transitionStartTime = performance.now();
    this.needsAnimation = true;
    if (this.orbitControls) {
      // this.orbitControls.enabled = false;
    }
  }
}
