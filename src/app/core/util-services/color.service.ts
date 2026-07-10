import { Injectable } from '@angular/core';
import * as THREE from 'three';

@Injectable({
  providedIn: 'root'
})
export class ColorService {
  private hue: number;

  constructor() {
    this.hue = Math.random();
  }

  public nextColor(): THREE.Color {
    this.hue += 0.618033988749895;
    this.hue %= 1;
    return new THREE.Color().setHSL(this.hue, 0.5, 0.5);
  }
}

