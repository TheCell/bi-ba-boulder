export interface SpraywallSaveData {
    imageData: string;
    spraywallId: string;
    name: string;
    description?: string | null;
    fontGrade?: number;
    existingId?: string;
    version?: number;
}