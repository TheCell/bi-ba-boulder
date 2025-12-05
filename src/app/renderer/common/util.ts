import * as THREE from 'three';

export function getImageDataFromTexture(texture: THREE.Texture<HTMLImageElement>): ImageData {
  const image = texture.image;
  const canvas = document.createElement('canvas');
  const ctx = canvas.getContext('2d');

  canvas.width = image.width;
  canvas.height = image.height;

  ctx!.drawImage(image, 0, 0);
  const imageData = ctx!.getImageData(0, 0, image.width, image.height);
  return imageData;
}

export function downloadSpraywallProblemImage(texture: THREE.DataTexture): void {
  if (texture.isTexture && texture.image && texture.image.data) {
    const canvas = document.createElement('canvas');
    const context = canvas.getContext('2d')!;
    const imgData = context.createImageData(texture.image.width, texture.image.height);
    canvas.width = texture.image.width;
    canvas.height = texture.image.height;

    for (let i = 0; i < texture.image.data.length; i += 4) {
      imgData.data[i] = texture.image.data[i];
      imgData.data[i + 1] = texture.image.data[i + 1];
      imgData.data[i + 2] = texture.image.data[i + 2];
      imgData.data[i + 3] = 255;
    }

    context.putImageData(imgData, 0, 0);

    const img = new Image();
    img.src = canvas.toDataURL('image/png');
    const link = document.createElement('a');
    link.href = img.src;
    link.download = `highlighted_route_${Date.now()}.png`;
    link.click();
    URL.revokeObjectURL(link.href);
  }
}

export function dumpObject(obj: THREE.Group<THREE.Object3DEventMap>, lines: string[] = [], isLast = true, prefix = '') {
  const localPrefix = isLast ? '└─' : '├─';
  lines.push(`${prefix}${prefix ? localPrefix : ''}${obj.name || '*no-name*'} [${obj.type}]`);
  const newPrefix = prefix + (isLast ? '  ' : '│ ');
  const lastNdx = obj.children.length - 1;
  obj.children.forEach((child, ndx) => {
    const isLast = ndx === lastNdx;
    dumpObject(child as THREE.Group<THREE.Object3DEventMap>, lines, isLast, newPrefix);
  });
  return lines;
}