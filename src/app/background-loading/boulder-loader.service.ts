import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { from, Observable, of } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';
import { RESOLUTION_LEVEL, ResolutionLevel } from '../interfaces/resolution-level';
import { BlocDto } from '@api-net/index';
import { environment } from '../../environments/environment';
import { UrlsAndExtras } from './boulder-loader-url.interface';

interface CachedBoulderRecord {
  key: string;
  blocId: string;
  url: string;
  data: ArrayBuffer;
  size: number;
  createdAt: number;
  lastAccessedAt: number;
}

@Injectable({
  providedIn: 'root'
})
export class BoulderLoaderService {
  private readonly http: HttpClient = inject(HttpClient);
  private readonly databaseName: string = 'boulder-cache-v1';
  private readonly databaseVersion: number = 1;
  private readonly idbSupported: boolean = typeof indexedDB !== 'undefined';
  private dbPromise: Promise<IDBDatabase> | undefined;
  private readonly budgetsByResolution: Record<ResolutionLevel, number> = {
    [RESOLUTION_LEVEL.low]: 160 * 1024 * 1024,
    [RESOLUTION_LEVEL.medium]: 320 * 1024 * 1024,
    [RESOLUTION_LEVEL.high]: 512 * 1024 * 1024
  };

  public loadBoulder(url: string, blocId: string, resolution: ResolutionLevel): Observable<ArrayBuffer> {
    const fullUrl = `${environment.boulderResourceURL}/${url}`;
    const cacheKey: string = this.createCacheKey(blocId, url);

    return this.loadArrayBufferWithCache(fullUrl, cacheKey, resolution, blocId, url);
  }

  public getUrls(blocDto: BlocDto, resolutionLevel?: ResolutionLevel): UrlsAndExtras {
    if (resolutionLevel === undefined) {
      resolutionLevel = this.getFirstResolution(blocDto);
    }

    if (resolutionLevel === undefined) {
      return { urls: [], blocIds: [], currentResolution: undefined };
    }

    return this.getUrl(blocDto, resolutionLevel);
  }

  public getNextResolution(blocDto: BlocDto, currentResolution?: ResolutionLevel): ResolutionLevel | undefined {
    switch (currentResolution) {
      case RESOLUTION_LEVEL.low:
        if (blocDto.blocMedRes !== undefined && blocDto.blocMedRes !== null) {
          return RESOLUTION_LEVEL.medium;
        }

        if (blocDto.blocHighRes !== undefined && blocDto.blocHighRes !== null) {
          return RESOLUTION_LEVEL.high;
        }

        return undefined;
      case RESOLUTION_LEVEL.medium:
        if (blocDto.blocHighRes !== undefined && blocDto.blocHighRes !== null) {
          return RESOLUTION_LEVEL.high;
        }

        return undefined;
      case RESOLUTION_LEVEL.high:
        return undefined;
      default:
        return undefined;
    }
  }

  private getUrl(blocDto: BlocDto, resolutionLevel: ResolutionLevel): UrlsAndExtras {
    switch (resolutionLevel) {
      case 'low':
        return {
          urls: [blocDto.blocLowRes, ...(blocDto.additionalParts?.map((part) => part.blocLowRes) ?? [])]
            .flat()
            .filter((url): url is string => url !== undefined && url !== null),
          blocIds: [blocDto.id, ...(blocDto.additionalParts?.map((part) => part.id) ?? [])],
          currentResolution: RESOLUTION_LEVEL.low
        };
      case 'medium':
        return {
          urls: [blocDto.blocMedRes, ...(blocDto.additionalParts?.map((part) => part.blocMedRes) ?? [])]
            .flat()
            .filter((url): url is string => url !== undefined && url !== null),
          blocIds: [blocDto.id, ...(blocDto.additionalParts?.map((part) => part.id) ?? [])],
          currentResolution: RESOLUTION_LEVEL.medium
        };
      case 'high':
        return {
          urls: [
            blocDto.blocHighRes,
            ...(blocDto.additionalParts?.map((part) => part.blocHighRes) ?? [])
            // .filter((part) => part.blocHighRes !== undefined && part.blocHighRes !== null)
          ]
            .flat()
            .filter((url): url is string => url !== undefined && url !== null),
          blocIds: [blocDto.id, ...(blocDto.additionalParts?.map((part) => part.id) ?? [])],
          currentResolution: RESOLUTION_LEVEL.high
        };
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

  public loadTestBoulder(): Observable<ArrayBuffer> {
    return this.loadWithCache(
      './api-test/boulder/bimano/bimano_low_pos_corrected.glb',
      this.createCacheKey('test-bimano', './api-test/boulder/bimano/bimano_low_pos_corrected.glb'),
      RESOLUTION_LEVEL.low
    );
  }

  public loadTestDaoneBoulder(resolutionLevel: ResolutionLevel): Observable<ArrayBuffer> {
    switch (resolutionLevel) {
      case 'low':
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb',
          this.createCacheKey('test-daone-1', './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb'),
          resolutionLevel
        );
      case 'medium':
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.002.glb',
          this.createCacheKey('test-daone-1', './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.002.glb'),
          resolutionLevel
        );
      case 'high':
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.001.glb',
          this.createCacheKey('test-daone-1', './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.001.glb'),
          resolutionLevel
        );
      default:
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb',
          this.createCacheKey('test-daone-1', './api-test/boulder/daone/la-plana/HIS_0110_Cleanup_reduced_0.0001.glb'),
          RESOLUTION_LEVEL.low
        );
    }
  }

  public loadTestDaoneBoulder2(resolutionLevel: ResolutionLevel): Observable<ArrayBuffer> {
    switch (resolutionLevel) {
      case 'low':
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.0011.glb',
          this.createCacheKey('test-daone-2', './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.0011.glb'),
          resolutionLevel
        );
      case 'medium':
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.01_tex_0.25.glb',
          this.createCacheKey('test-daone-2', './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.01_tex_0.25.glb'),
          resolutionLevel
        );
      case 'high':
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.02_tex_0.35.glb',
          this.createCacheKey('test-daone-2', './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.02_tex_0.35.glb'),
          resolutionLevel
        );
      default:
        return this.loadWithCache(
          './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.0011.glb',
          this.createCacheKey('test-daone-2', './api-test/boulder/daone/la-plana/HIS_0761_reduced_0.0011.glb'),
          RESOLUTION_LEVEL.low
        );
    }
  }

  public loadTestSpraywall(): Observable<ArrayBuffer> {
    return this.loadWithCache(
      './api-test/boulder/spraywall/Spraywall_separated_test_4096.glb',
      this.createCacheKey('test-spraywall-1', './api-test/boulder/spraywall/Spraywall_separated_test_4096.glb'),
      RESOLUTION_LEVEL.high
    );
  }

  public loadTestSpraywall2(): Observable<ArrayBuffer> {
    return this.loadWithCache(
      './api-test/boulder/spraywall2/Bimano_Spraywall_02_LOD0.glb',
      this.createCacheKey('test-spraywall-2', './api-test/boulder/spraywall2/Bimano_Spraywall_02_LOD0.glb'),
      RESOLUTION_LEVEL.high
    );
  }

  public loadTestSpraywall3(): Observable<ArrayBuffer> {
    return this.loadWithCache(
      './api-test/boulder/spraywall2/Bimano_Spraywall_2025_UV_shenanigans_03.glb',
      this.createCacheKey(
        'test-spraywall-3',
        './api-test/boulder/spraywall2/Bimano_Spraywall_2025_UV_shenanigans_03.glb'
      ),
      RESOLUTION_LEVEL.high
    );
  }

  private loadWithCache(fullUrl: string, cacheKey: string, resolution: ResolutionLevel): Observable<ArrayBuffer> {
    return this.loadArrayBufferWithCache(fullUrl, cacheKey, resolution, 'test', fullUrl);
  }

  private loadArrayBufferWithCache(
    fullUrl: string,
    cacheKey: string,
    resolution: ResolutionLevel,
    blocId: string,
    sourceUrl: string
  ): Observable<ArrayBuffer> {
    if (!this.idbSupported) {
      return this.http.get(fullUrl, { responseType: 'arraybuffer' });
    }

    return from(this.readFromCache(resolution, cacheKey)).pipe(
      switchMap((cached: ArrayBuffer | undefined) => {
        if (cached !== undefined) {
          return of(cached);
        }

        return this.http.get(fullUrl, { responseType: 'arraybuffer' }).pipe(
          tap((data: ArrayBuffer) => {
            void this.writeToCache(resolution, {
              key: cacheKey,
              blocId,
              url: sourceUrl,
              data,
              size: data.byteLength,
              createdAt: Date.now(),
              lastAccessedAt: Date.now()
            });
          })
        );
      })
    );
  }

  private createCacheKey(blocId: string, url: string): string {
    return `${blocId}::${url}`;
  }

  private readFromCache(resolution: ResolutionLevel, cacheKey: string): Promise<ArrayBuffer | undefined> {
    return this.openCacheDatabase()
      .then((database: IDBDatabase) =>
        this.getRecord(database, resolution, cacheKey).then((record) => ({ database, record }))
      )
      .then(({ database, record }: { database: IDBDatabase; record: CachedBoulderRecord | undefined }) => {
        if (record === undefined) {
          return undefined;
        }

        record.lastAccessedAt = Date.now();
        return this.putRecord(database, resolution, record).then(() => record.data);
      })
      .catch(() => undefined);
  }

  private writeToCache(resolution: ResolutionLevel, record: CachedBoulderRecord): Promise<void> {
    const budget: number = this.budgetsByResolution[resolution];
    if (record.size > budget) {
      return Promise.resolve();
    }

    return this.openCacheDatabase()
      .then((database: IDBDatabase) => this.putRecord(database, resolution, record).then(() => database))
      .then((database: IDBDatabase) => this.enforceBudget(database, resolution))
      .catch(() => Promise.resolve());
  }

  private openCacheDatabase(): Promise<IDBDatabase> {
    if (this.dbPromise !== undefined) {
      return this.dbPromise;
    }

    this.dbPromise = new Promise<IDBDatabase>((resolve, reject) => {
      const openRequest: IDBOpenDBRequest = indexedDB.open(this.databaseName, this.databaseVersion);

      openRequest.onupgradeneeded = () => {
        const database: IDBDatabase = openRequest.result;
        for (const resolution of this.getResolutions()) {
          if (!database.objectStoreNames.contains(resolution)) {
            database.createObjectStore(resolution, { keyPath: 'key' });
          }
        }
      };

      openRequest.onsuccess = () => {
        resolve(openRequest.result);
      };

      openRequest.onerror = () => {
        reject(openRequest.error ?? new Error('IndexedDB open failed'));
      };
    });

    return this.dbPromise;
  }

  private getResolutions(): ResolutionLevel[] {
    return [RESOLUTION_LEVEL.low, RESOLUTION_LEVEL.medium, RESOLUTION_LEVEL.high];
  }

  private getRecord(
    database: IDBDatabase,
    resolution: ResolutionLevel,
    cacheKey: string
  ): Promise<CachedBoulderRecord | undefined> {
    const transaction: IDBTransaction = database.transaction(resolution, 'readonly');
    const objectStore: IDBObjectStore = transaction.objectStore(resolution);
    const request: IDBRequest<CachedBoulderRecord | undefined> = objectStore.get(cacheKey);

    return this.awaitRequest(request).then((record: CachedBoulderRecord | undefined) =>
      this.awaitTransaction(transaction).then(() => record)
    );
  }

  private putRecord(database: IDBDatabase, resolution: ResolutionLevel, record: CachedBoulderRecord): Promise<void> {
    const transaction: IDBTransaction = database.transaction(resolution, 'readwrite');
    const objectStore: IDBObjectStore = transaction.objectStore(resolution);
    const request: IDBRequest<IDBValidKey> = objectStore.put(record);

    return this.awaitRequest(request).then(() => this.awaitTransaction(transaction));
  }

  private getAllRecords(database: IDBDatabase, resolution: ResolutionLevel): Promise<CachedBoulderRecord[]> {
    const transaction: IDBTransaction = database.transaction(resolution, 'readonly');
    const objectStore: IDBObjectStore = transaction.objectStore(resolution);
    const request: IDBRequest<CachedBoulderRecord[]> = objectStore.getAll();

    return this.awaitRequest(request).then((records: CachedBoulderRecord[]) =>
      this.awaitTransaction(transaction).then(() => records)
    );
  }

  private enforceBudget(database: IDBDatabase, resolution: ResolutionLevel): Promise<void> {
    const budget: number = this.budgetsByResolution[resolution];

    return this.getAllRecords(database, resolution).then((records: CachedBoulderRecord[]) => {
      let totalSize: number = records.reduce((sum: number, record: CachedBoulderRecord) => sum + record.size, 0);

      if (totalSize <= budget) {
        return Promise.resolve();
      }

      records.sort(
        (left: CachedBoulderRecord, right: CachedBoulderRecord) => left.lastAccessedAt - right.lastAccessedAt
      );

      const transaction: IDBTransaction = database.transaction(resolution, 'readwrite');
      const objectStore: IDBObjectStore = transaction.objectStore(resolution);

      for (const record of records) {
        if (totalSize <= budget) {
          break;
        }

        objectStore.delete(record.key);
        totalSize -= record.size;
      }

      return this.awaitTransaction(transaction);
    });
  }

  private awaitRequest<T>(request: IDBRequest<T>): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error ?? new Error('IndexedDB request failed'));
    });
  }

  private awaitTransaction(transaction: IDBTransaction): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      transaction.oncomplete = () => resolve();
      transaction.onerror = () => reject(transaction.error ?? new Error('IndexedDB transaction failed'));
      transaction.onabort = () => reject(transaction.error ?? new Error('IndexedDB transaction aborted'));
    });
  }
}
