import * as THREE from 'three';

export interface Viewpoint {
  position: THREE.Vector3;
  target: THREE.Vector3;
}
