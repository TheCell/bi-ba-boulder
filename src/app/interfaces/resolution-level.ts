export const RESOLUTION_LEVEL = {
  low: 'low',
  medium: 'medium',
  high: 'high'
} as const;

export type ResolutionLevel = keyof typeof RESOLUTION_LEVEL;
