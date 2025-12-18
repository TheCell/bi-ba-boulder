export const vViewPositionReplace = [
    'varying vec3 vViewPosition;',
    'attribute vec2 uv1;',
    'uniform float time;',
    'varying vec2 vUv1;',
    'varying float fresnel;'
];

export const beginVertex = [
    '#include <begin_vertex>',
    'vUv1 = vec3( uv1, 1 ).xy;',
];

export const uniforms = [
    'uniform float opacity;',
    'uniform float useRgbTexture;',
    'uniform sampler2D rgbTexture;',
    'uniform sampler2D highlightedHoldsTexture;',
    'varying vec2 vUv1;',
    'varying float fresnel;'
];

export const worldposVertex = [
    '#if defined( USE_ENVMAP ) || defined( DISTANCE ) || defined ( USE_SHADOWMAP ) || defined ( USE_TRANSMISSION ) || NUM_SPOT_LIGHT_COORDS > 0',
    '	vec4 worldPosition = vec4( transformed, 1.0 );',
    '	#ifdef USE_BATCHING',
    '		worldPosition = batchingMatrix * worldPosition;',
    '	#endif',
    '	#ifdef USE_INSTANCING',
    '		worldPosition = instanceMatrix * worldPosition;',
    '	#endif',
    '	worldPosition = modelMatrix * worldPosition;',
    '#endif',
    'float amount = 0.5;',
    'fresnel = pow((1.0 - clamp(dot(normalize(normal), normalize(vViewPosition)), 0.0, 1.0)), amount);',
];

export const mapFragment = [
    '#ifdef USE_MAP',
    'vec4 sampledDiffuseColor = useRgbTexture > 0.0 ? texture2D( rgbTexture, vUv1 ) : texture2D( map, vMapUv );',
    'vec4 highlightedHoldsColor = texture2D( highlightedHoldsTexture, vUv1 );',
    '#ifdef DECODE_VIDEO_TEXTURE',
    'sampledDiffuseColor = sRGBTransferEOTF( sampledDiffuseColor );',
    '#endif',
    // 'diffuseColor *= sampledDiffuseColor;',
    'float hasHighlight = step(0.0001, highlightedHoldsColor.r + highlightedHoldsColor.g + highlightedHoldsColor.b);',
    'hasHighlight = clamp(hasHighlight * (1.0 - step(0.5, useRgbTexture)), 0.0, 1.0);',
    'vec3 highlightColor = highlightedHoldsColor.rgb * fresnel;',
    'vec4 fresnelGroundColor = vec4(0.2, 0.2, 0.2, 1.0);',
    'diffuseColor = mix(fresnelGroundColor, sampledDiffuseColor, 1.0 - hasHighlight);',
    'totalEmissiveRadiance.rgb = mix(totalEmissiveRadiance.rgb, highlightColor, hasHighlight);',
    '#endif'
]