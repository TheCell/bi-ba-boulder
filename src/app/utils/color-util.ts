export function HSLToHex(hsl: { h: number; s: number; l: number }): string {
  const { h, s, l } = hsl;

  const hDecimal = l / 100;
  const a = (s * Math.min(hDecimal, 1 - hDecimal)) / 100;
  const f = (n: number) => {
    const k = (n + h / 30) % 12;
    const color = hDecimal - a * Math.max(Math.min(k - 3, 9 - k, 1), -1);

    // Convert to Hex and prefix with "0" if required
    return Math.round(255 * color)
      .toString(16)
      .padStart(2, "0");
  };
  return `#${f(0)}${f(8)}${f(4)}`;
}

export function HexToHSL(hex: string): { h: number; s: number; l: number } {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);

  if (!result) {
    throw new Error("Could not parse Hex Color");
  }

  const rHex = parseInt(result[1], 16);
  const gHex = parseInt(result[2], 16);
  const bHex = parseInt(result[3], 16);

  const r = rHex / 255;
  const g = gHex / 255;
  const b = bHex / 255;

  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);

  let h = (max + min) / 2;
  let s = h;
  let l = h;

  if (max === min) {
    // Achromatic
    return { h: 0, s: 0, l };
  }

  const d = max - min;
  s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
  switch (max) {
    case r:
      h = (g - b) / d + (g < b ? 6 : 0);
      break;
    case g:
      h = (b - r) / d + 2;
      break;
    case b:
      h = (r - g) / d + 4;
      break;
  }
  h /= 6;

  s = s * 100;
  s = Math.round(s);
  l = l * 100;
  l = Math.round(l);
  h = Math.round(360 * h);

  return { h, s, l };
}

export function HSLToRGB(hsl: {h: number; s: number; l: number;}): { r: number; g: number; b: number } {
  const { h, s, l } = hsl;

  const hDecimal = h / 100;
  const sDecimal = s / 100;
  const lDecimal = l / 100;

  if (s === 0) {
    return { r: lDecimal, g: lDecimal, b: lDecimal };
  }

  const HueToRGB = (p: number, q: number, t: number) => {
    if (t < 0) t += 1;
    if (t > 1) t -= 1;
    if (t < 1 / 6) return p + (q - p) * 6 * t;
    if (t < 1 / 2) return q;
    if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
    return p;
  };

  const q =
    lDecimal < 0.5
      ? lDecimal * (1 + sDecimal)
      : lDecimal + sDecimal - lDecimal * sDecimal;
  const p = 2 * lDecimal - q;

  const r = HueToRGB(p, q, hDecimal + 1 / 3);
  const g = HueToRGB(p, q, hDecimal);
  const b = HueToRGB(p, q, hDecimal - 1 / 3);

  return { r: r * 255, g: g * 255, b: b * 255 };
}

export function RGBToHSL(rgb: { r: number; g: number; b: number; }): {
  h: number;
  s: number;
  l: number;
} {
  const { r: r255, g: g255, b: b255 } = rgb;

  const r = r255 / 255;
  const g = g255 / 255;
  const b = b255 / 255;

  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);

  let h = (max + min) / 2;
  let s = h;
  const l = h;

  if (max === min) {
    // Achromatic
    return { h: 0, s: 0, l };
  }

  const d = max - min;
  s = l >= 0.5 ? d / (2 - (max + min)) : d / (max + min);
  switch (max) {
    case r:
      h = ((g - b) / d + 0) * 60;
      break;
    case g:
      h = ((b - r) / d + 2) * 60;
      break;
    case b:
      h = ((r - g) / d + 4) * 60;
      break;
  }

  return { h: Math.round(h), s: Math.round(s * 100), l: Math.round(l * 100) };
}
