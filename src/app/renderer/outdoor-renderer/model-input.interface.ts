import { ResolutionLevel } from '../../interfaces/resolution-level';

export interface RawModelInput {
  arrayBuffer: ArrayBuffer;
  resolution: ResolutionLevel;
  blocId: string;
}
