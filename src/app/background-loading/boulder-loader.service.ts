import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RESOLUTION_LEVEL, ResolutionLevel } from '../interfaces/resolution-level';
import { BlocDto } from '../api';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class BoulderLoaderService {

  public constructor(private http: HttpClient) { }

  public loadBoulder(url: string): Observable<ArrayBuffer> {
    return this.http.get(`${environment.boulderResourceURL}/${url}`, { responseType: 'arraybuffer'});
  }

  public getUrl(blocDto: BlocDto, resolutionLevel?: ResolutionLevel): { url: string, higherResolution: ResolutionLevel | undefined } {
    if (resolutionLevel === undefined) {
      resolutionLevel = this.getFirstResolution(blocDto);
    }

    switch (resolutionLevel) {
      case 'low':
        return { url: blocDto.blocLowRes!, higherResolution: this.getNextResolution(blocDto, RESOLUTION_LEVEL.low) };
      case 'medium':
        return { url: blocDto.blocMedRes!, higherResolution: this.getNextResolution(blocDto, RESOLUTION_LEVEL.medium) };
      case 'high':
        return { url: blocDto.blocHighRes!, higherResolution: this.getNextResolution(blocDto, RESOLUTION_LEVEL.high) };
      default:
        return { url: '', higherResolution: undefined };
    }
  }

  private getNextResolution(blocDto: BlocDto, currentResolution?: ResolutionLevel): ResolutionLevel | undefined {
    switch (currentResolution) {
      case RESOLUTION_LEVEL.low:
        if (blocDto.blocMedRes !== undefined) {
          return RESOLUTION_LEVEL.medium;
        }

        if (blocDto.blocHighRes !== undefined) {
          return RESOLUTION_LEVEL.high;
        }

        return undefined;
      case RESOLUTION_LEVEL.medium:
        if (blocDto.blocHighRes !== undefined) {
          return RESOLUTION_LEVEL.high;
        }

        return undefined;
      case RESOLUTION_LEVEL.high:
        return undefined;
      default:
        return undefined;
    }
  }

  private getFirstResolution(blocDto: BlocDto): ResolutionLevel | undefined {
    if (blocDto.blocLowRes !== undefined) {
      return RESOLUTION_LEVEL.low;
    }

    if (blocDto.blocMedRes !== undefined) {
      return RESOLUTION_LEVEL.medium;
    }

    if (blocDto.blocHighRes !== undefined) {
      return RESOLUTION_LEVEL.high;
    }

    return undefined;
  }

  // public loadBoulder(resolutionLevel: ResolutionLevel): Observable<ArrayBuffer> {
  //   switch (resolutionLevel) {
  //     case 'low':
  //       return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb', { responseType: 'arraybuffer'});
  //     case 'medium':
  //       return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.002.glb', { responseType: 'arraybuffer'});
  //       case 'high':
  //       return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.001.glb', { responseType: 'arraybuffer'});
  //       default:
  //       return this.http.get('./api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb', { responseType: 'arraybuffer'});
  //   }
  // }

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

  public loadTestSpraywall(): Observable<ArrayBuffer> {
    return this.http.get('./api-test/boulder/spraywall/Spraywall_separated_test_4096.glb', { responseType: 'arraybuffer'});
  }

  public loadTestSpraywall2(): Observable<ArrayBuffer> {
    return this.http.get('./api-test/boulder/spraywall2/Bimano_Spraywall_02_LOD0.glb', { responseType: 'arraybuffer'});
  }

}
