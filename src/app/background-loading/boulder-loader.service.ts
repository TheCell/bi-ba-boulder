import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ResolutionLevel } from '../interfaces/resolution-level';

@Injectable({
  providedIn: 'root'
})
export class BoulderLoaderService {

  public constructor(private http: HttpClient) { }

  public loadTestBoulder(): Observable<ArrayBuffer> {
    return this.http.get('./api-test/boulder/bimano/bimano_low_pos_corrected.glb', { responseType: 'arraybuffer'});
  }

  public loadTestDaoneBoulder(resolutionLevel: ResolutionLevel): Observable<ArrayBuffer> {
    switch (resolutionLevel) {
      case 'low':
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb', { responseType: 'arraybuffer'});
      case 'medium':
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.002.glb', { responseType: 'arraybuffer'});
        case 'high':
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.001.glb', { responseType: 'arraybuffer'});
        default:
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb', { responseType: 'arraybuffer'});
    }
  }

  public loadTestDaoneBoulder2(resolutionLevel: ResolutionLevel): Observable<ArrayBuffer> {
    switch (resolutionLevel) {
      case 'low':
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0761_reduced_0.0011.glb', { responseType: 'arraybuffer'});
      case 'medium':
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0761_reduced_0.01_tex_0.25.glb', { responseType: 'arraybuffer'});
        case 'high':
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0761_reduced_0.02_tex_0.35.glb', { responseType: 'arraybuffer'});
        default:
        return this.http.get('./api-test/boulder/daone/la-plana/HIS_0761_reduced_0.0011.glb', { responseType: 'arraybuffer'});
    }
  }

}
