export interface SpraywallSaveData {
  imageData: string;
  spraywallId: string;
  name: string;
  description?: string | null;
  fontGrade?: number;
  isCircuit?: boolean;
  noMatch?: boolean;
  freeFeet?: boolean;
  existingId?: string;
  version?: number;
}
