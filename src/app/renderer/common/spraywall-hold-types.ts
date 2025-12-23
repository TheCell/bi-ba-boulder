import * as THREE from 'three';

export enum SpraywallHoldType {
  undefined = 0,
  start = 1,
  top = 2,
  hold = 3,
  foot = 4,
  custom = 5
};


export interface TypeAndColor {
  type: SpraywallHoldType;
  color: THREE.Color;
}

export const holdColorOptions: TypeAndColor[] = [
  { type: SpraywallHoldType.start, color: new THREE.Color(128, 149, 37) },
  { type: SpraywallHoldType.top, color: new THREE.Color(213, 94, 0) },
  { type: SpraywallHoldType.hold, color: new THREE.Color(86, 180, 233) },
  { type: SpraywallHoldType.foot, color: new THREE.Color(240, 228, 66) },
  { type: SpraywallHoldType.custom, color: new THREE.Color(204, 121, 167) }
];