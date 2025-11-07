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
    // `float theta = 1.0 + sin( time ) / ${ 1.1.toFixed(1) };`,
    // 'transformed.x *= theta;',
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
    // 'fresnel = abs(dot(normalize(vViewPosition), normal));',
    // 'fresnel = dot(normalize(vViewPosition), normal);',
    'float amount = 0.5;',
    'fresnel = pow((1.0 - clamp(dot(normalize(normal), normalize(vViewPosition)), 0.0, 1.0)), amount);',
    // 'fresnel = normal.z;',
    // 'fresnel = dot( normalize( vViewPosition ), normal );',
    // 'fresnel = dot( normalize( vViewPosition ), normal );',
    // 'fresnel = dot(normalize(vViewPosition), normal);',
];

export const opacity = [
    'uniform float opacity;',
    'uniform float useRgbTexture;',
    'uniform sampler2D rgbTexture;',
    'uniform sampler2D highlightedHoldsTexture;',
    'varying vec2 vUv1;',
    'varying float fresnel;'
];

export const mapFragment = [
    '#ifdef USE_MAP',
    'vec4 sampledDiffuseColor = useRgbTexture > 0.0 ? texture2D( rgbTexture, vUv1 ) : texture2D( map, vMapUv );',
    'vec4 highlightedHoldsColor = texture2D( highlightedHoldsTexture, vUv1 );',
    '#ifdef DECODE_VIDEO_TEXTURE',
    'sampledDiffuseColor = sRGBTransferEOTF( sampledDiffuseColor );',
    '#endif',
    'diffuseColor *= sampledDiffuseColor;',
    'float hasHighlight = step(0.0, highlightedHoldsColor.r + highlightedHoldsColor.g + highlightedHoldsColor.b);',
    'hasHighlight = clamp(hasHighlight * (1.0 - step(0.5, useRgbTexture)), 0.0, 1.0);',
    'vec3 sampledGray = vec3((sampledDiffuseColor.r + sampledDiffuseColor.g + sampledDiffuseColor.b) / 3.0);',
    'vec3 baseColor = diffuseColor.rgb * (1.0 - fresnel) + sampledGray * fresnel;',
    'vec3 highlightColor = highlightedHoldsColor.rgb * fresnel;',
    'totalEmissiveRadiance.rgb = mix(totalEmissiveRadiance.rgb, highlightColor, hasHighlight);',
    // 'diffuseColor.rgb = mix(diffuseColor.rgb, diffuseColor.rgb * vec3(1.0 - fresnel), hasHighlight);',
    // 'if (useRgbTexture <= 0.0 && (highlightedHoldsColor.r > 0.0 || highlightedHoldsColor.g > 0.0 || highlightedHoldsColor.b > 0.0)) {',
    // 'vec3 sampledGray = vec3((sampledDiffuseColor.r + sampledDiffuseColor.g + sampledDiffuseColor.b) / 3.0);',
    // 'diffuseColor.rgb = diffuseColor.rgb * (1.0 - fresnel) + sampledGray * fresnel;',
    // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * fresnel;',
    // 'diffuseColor.rgb = diffuseColor.rgb * vec3(1.0 - fresnel);',
    // '',
    // 'float grid_position = rand( vMapUv.xy );',
    // 'float grid_position = clamp(rand( vMapUv.xy ) - 0.5, 0.0, 1.0);',
    // 'vec3 truecolor = vec3(highlightedHoldsColor.r * grid_position + diffuseColor.r * (1.0 - grid_position), highlightedHoldsColor.g * grid_position + diffuseColor.g * (1.0 - grid_position), highlightedHoldsColor.b * grid_position + diffuseColor.b * (1.0 - grid_position));',
    // 'diffuseColor.rgb = truecolor;',
    // 'totalEmissiveRadiance  = truecolor;',
    // 'vec3 mixedHighlight = vec3(highlightedHoldsColor.r * grid_position, highlightedHoldsColor.g * grid_position, highlightedHoldsColor.b * grid_position);',
    // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * sampledGray;',
    // 'float normaliedFresnel = clamp( (fresnel - 0.5) * 2.0, 0.0, 1.0);',
    // 'float clampedFresnel = clamp(fresnel, 0.0, 1.0);',
    // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * (1.0 - fresnel) * 0.5;',
    // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * (1.0 - clampedFresnel) * 0.5;',
    // 'totalEmissiveRadiance.rgb = vec3(0.0);',
    // 'diffuseColor.rgb = vec3(fresnel);',
    // 'totalEmissiveRadiance.rgb = highlightedHoldsColor.rgb * (1.0 - fresnel);',
    // 'totalEmissiveRadiance.rgb = vec3(1.0 - fresnel);',
    // 'totalEmissiveRadiance.rgb = vec3(fresnel);',
    // 'diffuseColor.rgb = vec3(1.0 - fresnel);',
    // 'diffuseColor.rgb = diffuseColor.rgb * vec3(1.0 - fresnel);',
    // 'diffuseColor.rgb = highlightedHoldsColor.rgb;',
    // 'diffuseColor.rgb *= vec3(2.0);',
    // '}',
    '#endif'
]