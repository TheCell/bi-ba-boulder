import { ResolutionLevel } from '../interfaces/resolution-level';

export interface UrlsAndExtras {
  urls: string[];
  blocIds: string[];
  currentResolution: ResolutionLevel | undefined;
}
