import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ImportPreview, ImportResult } from './import.models';

@Injectable({ providedIn: 'root' })
export class ImportApiService {
  private readonly http = inject(HttpClient);
  preview(sourceUrl: string): Promise<ImportPreview> {
    return firstValueFrom(this.http.post<ImportPreview>('/api/import/preview', { sourceUrl }));
  }
  confirm(sourceUrl: string): Promise<ImportResult> {
    return firstValueFrom(this.http.post<ImportResult>('/api/import/confirm', { sourceUrl }));
  }
}
