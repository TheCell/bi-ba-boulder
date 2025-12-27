export interface SpraywallSaveData {
    imageData: string;
    spraywallId: string;
    name: string;
    description?: string | null;
    fontGrade?: number | null;
    existingId?: string;
}