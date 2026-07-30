export interface SpraywallSaveData {
  imageData: string;
  spraywallId: string;
  name: string;
  description?: string | null;
  fontGrade?: number;
  isCircuit?: boolean;
  noMatch?: boolean;
  freeFeet?: boolean;
  isWip?: boolean;
  existingId?: string;
  version?: number;
}
