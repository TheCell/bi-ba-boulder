import * as THREE from 'three';

export function createHelperOverlayTexture(): THREE.DataTexture {
  const data: Uint8Array = new Uint8Array([0, 0, 0, 255]);
  const texture: THREE.DataTexture = new THREE.DataTexture(data, 1, 1, THREE.RGBAFormat);
  texture.needsUpdate = true;
  return texture;
}
