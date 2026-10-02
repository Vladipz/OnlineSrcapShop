import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ImportResult } from './import.models';

@Injectable({ providedIn: 'root' })
export class ImportApiService {
  private readonly http = inject(HttpClient);
  import(sourceUrl: string): Promise<ImportResult> {
    return firstValueFrom(this.http.post<ImportResult>('/api/import', { sourceUrl }));
  }
}
