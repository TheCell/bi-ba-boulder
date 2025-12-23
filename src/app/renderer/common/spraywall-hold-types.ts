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
  { type: SpraywallHoldType.start, color: new THREE.Color(104 / 255, 236 / 255, 105 / 255) },
  { type: SpraywallHoldType.top, color: new THREE.Color(213 / 255, 94 / 255, 0 / 255) },
  { type: SpraywallHoldType.hold, color: new THREE.Color(86 / 255, 180 / 255, 233 / 255) },
  { type: SpraywallHoldType.foot, color: new THREE.Color(240 / 255, 228 / 255, 66 / 255) },
  { type: SpraywallHoldType.custom, color: new THREE.Color(204 / 255, 121 / 255, 167 / 255) }
];