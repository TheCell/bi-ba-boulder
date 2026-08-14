export const maxSphereMarkings = 32;
export const maxBoxMarkings = 32;
export const helperLayer = 2;
export const spherePlacementRadius = 0.35;
export const boxPlacementWidth = 1.0;
export const boxPlacementHeight = 0.65;
export const boxPlacementDepth = 0.65;

export const uniforms = (maxSphereMarkings: number, maxBoxMarkings: number) => {
  return [
    'uniform float opacity;',
    'uniform int maxSphereMarkings;',
    'uniform int maxBoxMarkings;',
    'uniform sampler2D helperOverlayTexture;',
    'uniform int sphereMarkingCount;',
    `uniform vec4 sphereMarkings[${maxSphereMarkings}];`,
    `uniform vec3 sphereMarkingColors[${maxSphereMarkings}];`,
    'uniform int boxMarkingCount;',
    `uniform vec3 boxMarkingPositions[${maxBoxMarkings}];`,
    `uniform vec4 boxMarkingQuaternions[${maxBoxMarkings}];`,
    `uniform vec3 boxMarkingSizes[${maxBoxMarkings}];`,
    `uniform vec3 boxMarkingColors[${maxBoxMarkings}];`,
    'uniform float helperBlendStrength;',
    'uniform float helperEmissiveStrength;',
    'uniform float sphereFalloff;',
    'uniform float boxEdgeFalloff;',
    'varying vec3 vWorldPosition;',
    'vec3 rotateByQuaternion(vec3 vector, vec4 quaternion) {',
    '  return vector + 2.0 * cross(quaternion.xyz, cross(quaternion.xyz, vector) + quaternion.w * vector);',
    '}',
    'vec3 inverseRotateByQuaternion(vec3 vector, vec4 quaternion) {',
    '  return rotateByQuaternion(vector, vec4(-quaternion.xyz, quaternion.w));',
    '}'
  ];
};

export const fragmentShader = [
  '#include <map_fragment>',
  'float helperMask = 0.0;',
  'float helperWeightSum = 0.0;',
  'vec3 helperColorSum = vec3(0.0);',
  'int sphereMarkingCount = min(sphereMarkingCount, maxSphereMarkings);',
  `for (int i = 0; i < sphereMarkingCount; i++) {`,
  '  vec4 sphereMarking = sphereMarkings[i];',
  '  float sphereInfluence = 1.0 - smoothstep(sphereMarking.w, sphereMarking.w + sphereFalloff, distance(vWorldPosition, sphereMarking.xyz));',
  '  helperMask = max(helperMask, sphereInfluence);',
  '  helperWeightSum += sphereInfluence;',
  '  helperColorSum += sphereMarkingColors[i] * sphereInfluence;',
  '}',
  'int boxMarkingCount = min(boxMarkingCount, maxBoxMarkings);',
  `for (int i = 0; i < boxMarkingCount; i++) {`,
  '  vec3 localPosition = inverseRotateByQuaternion(vWorldPosition - boxMarkingPositions[i], boxMarkingQuaternions[i]);',
  '  vec3 halfSize = boxMarkingSizes[i] * 0.5;',
  '  vec3 outside = max(abs(localPosition) - halfSize, vec3(0.0));',
  '  float outsideDistance = length(outside);',
  '  float insideDistance = min(min(halfSize.x - abs(localPosition.x), halfSize.y - abs(localPosition.y)), halfSize.z - abs(localPosition.z));',
  '  float signedDistance = outsideDistance > 0.0 ? outsideDistance : -insideDistance;',
  '  float boxInfluence = 1.0 - smoothstep(0.0, boxEdgeFalloff, max(signedDistance, 0.0));',
  '  helperMask = max(helperMask, boxInfluence);',
  '  helperWeightSum += boxInfluence;',
  '  helperColorSum += boxMarkingColors[i] * boxInfluence;',
  '}',
  'helperMask = clamp(helperMask, 0.0, 1.0);',
  'vec3 averagedHelperColor = helperWeightSum > 0.0 ? helperColorSum / helperWeightSum : vec3(0.0);',
  'vec3 helperTextureColor = texture2D(helperOverlayTexture, vMapUv).rgb;',
  'vec3 helperBlendColor = helperTextureColor + averagedHelperColor;',
  'diffuseColor.rgb = mix(diffuseColor.rgb, helperBlendColor, helperMask * helperBlendStrength);',
  'totalEmissiveRadiance += helperBlendColor * helperMask * helperEmissiveStrength;'
];
