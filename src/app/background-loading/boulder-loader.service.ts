import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class BoulderLoaderService {

  public constructor(private http: HttpClient) { }

  public loadTestBoulder(): Observable<ArrayBuffer> {
    return this.http.get('./api-test/boulder/bimano/bimano_low_pos_corrected.glb', { responseType: 'arraybuffer'});
  }

}
