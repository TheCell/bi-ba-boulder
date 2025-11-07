import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class DebugLoader {
  private http: HttpClient = inject(HttpClient);

  constructor() { }


  // public loadSpraywallDebugTexture(): Observable<ArrayBuffer> {
  //   return this.http.get('./images/highlight_debug.png', { responseType: 'arraybuffer'});
  // }
}
