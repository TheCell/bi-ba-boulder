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