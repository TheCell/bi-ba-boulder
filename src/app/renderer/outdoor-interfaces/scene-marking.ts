import * as THREE from 'three';

export interface BaseSceneMarking {
  id: string;
  color: THREE.Color;
  mesh: THREE.Mesh;
  type: 'sphere' | 'box';
}
export interface SphereSceneMarking extends BaseSceneMarking {
  type: 'sphere';
}

export interface BoxSceneMarking extends BaseSceneMarking {
  type: 'box';
}

export type CustomSceneMarking = SphereSceneMarking | BoxSceneMarking;

export interface HelperShaderSnapshot {
  sphereMarkingCount: number;
  boxMarkingCount: number;
}

export type MaterialShader = Parameters<THREE.Material['onBeforeCompile']>[0];
