import * as THREE from 'three';

export function getImageDataFromTexture(texture: THREE.Texture): ImageData {
  const image = texture.image;
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');

  canvas.width = image.width;
  canvas.height = image.height;

  ctx!.drawImage(image, 0, 0);
  const imageData = ctx!.getImageData(0, 0, image.width, image.height);

  // console.log('getImageDataFromTexture', imageData);

  return imageData;
}

export function prepareHighlightDebugTexture(imageData: ArrayBuffer): THREE.Texture {
  const texture = new THREE.Texture();
  texture.image = new Image();
  texture.image.src = URL.createObjectURL(new Blob([imageData]));
  texture.flipY = false;
  texture.minFilter = THREE.NearestFilter;
  texture.magFilter = THREE.NearestFilter;
  texture.needsUpdate = true;
  return texture;
}

export function downloadSpraywallProblemImage(texture: THREE.Texture): void {
  if (texture.isTexture && texture.image && texture.image.data) {
    let canvas = document.createElement('canvas');
    let context = canvas.getContext('2d')!;
    let imgData = context.createImageData(texture.image.width, texture.image.height);
    canvas.width = texture.image.width;
    canvas.height = texture.image.height;

    for (let i = 0; i < texture.image.data.length; i += 4) {
      imgData.data[i] = texture.image.data[i];
      imgData.data[i + 1] = texture.image.data[i + 1];
      imgData.data[i + 2] = texture.image.data[i + 2];
      imgData.data[i + 3] = 255;
    }

    context.putImageData(imgData, 0, 0);

    let img = new Image();
    img.src = canvas.toDataURL('image/png');
    const link = document.createElement('a');
    link.href = img.src;
    link.download = `highlighted_route_${Date.now()}.png`;
    link.click();
    URL.revokeObjectURL(link.href);
  }
}