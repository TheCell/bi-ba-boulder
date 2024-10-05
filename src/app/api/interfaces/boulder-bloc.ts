import { BoulderLine } from "./boulder-line";

export interface BoulderBloc {
  id: string;
  boulderSectorId: string;
  name: string;
  description?: string;
  boulderLines: BoulderLine[];
}
