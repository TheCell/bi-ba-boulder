import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { BoulderSector } from '../api/interfaces/boulder-sector';

@Injectable({
  providedIn: 'root'
})
export class SectorLoaderService {

  public constructor(private http: HttpClient) { }

  public getTestSector(id: string): Observable<BoulderSector> {
    console.log(id);

    const sector: BoulderSector = {
      id: '64b45193-594c-40d5-8fcd-033e017a1c7b',
      name: 'La Plana',
      description: 'La Plana is a sector in Daone',
      boulderBlocIds: ['8d7fb0fe-8ef8-4086-97e0-51d44de2af83', '9ef9bf37-a629-4839-aee4-0b9d3dd8cb2b']
    }

    return of(sector);
  }

}
