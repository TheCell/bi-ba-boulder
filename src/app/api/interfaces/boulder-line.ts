import { FontGrade } from "./font-grade";

export interface BoulderLine {
  boulderBlocId: string;
  id: string;
  points: Array<{ x: number, y: number, z: number }>;
  color: string;
  name: string;
  grade: FontGrade;
  description?: string;
  date?: Date;
}
