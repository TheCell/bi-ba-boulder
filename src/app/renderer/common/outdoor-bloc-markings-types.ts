import * as THREE from 'three';

export enum OutdoorBlocMarkingsType {
  undefined = 0,
  start = 1,
  top = 2,
  offLineZone = 3
}

export interface OutdoorMarkingTypeAndColor {
  type: OutdoorBlocMarkingsType;
  color: THREE.Color;
}

export const outdoorBlocMarkingColorOptions: OutdoorMarkingTypeAndColor[] = [
  { type: OutdoorBlocMarkingsType.start, color: new THREE.Color(104 / 255, 236 / 255, 105 / 255) },
  { type: OutdoorBlocMarkingsType.top, color: new THREE.Color(213 / 255, 94 / 255, 0 / 255) },
  { type: OutdoorBlocMarkingsType.offLineZone, color: new THREE.Color(204 / 255, 121 / 255, 167 / 255) }
];
