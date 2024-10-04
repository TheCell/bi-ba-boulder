import { Vector3 } from "three";
import { FontGrade } from "./font-grade";

export interface BoulderLine {
  points: Array<{ x: number, y: number, z: number }>;
  color: string;
  name: string;
  grade: FontGrade;
  date?: Date;
}
