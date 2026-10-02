import { BlocDto, SectorDto } from '@api-net/index';

export interface BlocDialogData {
  bloc?: BlocDto;
  sectorId?: string;
  sectors?: SectorDto[];
}
