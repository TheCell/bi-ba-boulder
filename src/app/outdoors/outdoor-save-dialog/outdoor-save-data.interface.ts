import { LineData } from '@api-net/index';

export interface OutdoorSaveData {
  lineData: LineData;
  blocId: string;
  existingId?: string;
  name?: string;
  description?: string | null;
  identifier?: string;
  fontGrade?: number;
  version?: number;
}
