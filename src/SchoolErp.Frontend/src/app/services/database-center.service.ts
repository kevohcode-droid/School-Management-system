import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TableInfo {
  name: string;
  entityType: string;
  recordCount: number;
  sizeKb: number;
}

export interface RecordData {
  id?: string;
  _id?: string;
  [key: string]: any;
}

export interface PaginatedRecords {
  records: RecordData[];
  totalCount: number;
}

export interface TableStatistics {
  recordCount: number;
  tableSizeBytes: number;
  indexCount: number;
  indexes: Array<{ name: string; columns: string[] }>;
}

export type ImportStrategy = 'insert' | 'update' | 'merge' | 'ignore';

export interface ImportResult {
  total: number;
  imported: number;
  updated: number;
  skipped: number;
  errors: string[];
  duplicates: string[];
}

@Injectable({
  providedIn: 'root'
})
export class DatabaseCenterService {
  private baseUrl = '/api/database';

  constructor(private http: HttpClient) {}

  getTables(): Observable<TableInfo[]> {
    return this.http.get<TableInfo[]>(`${this.baseUrl}/tables`);
  }

  getRecords(tableName: string, params: any): Observable<PaginatedRecords> {
    let httpParams = new HttpParams();
    Object.keys(params).forEach(key => {
      httpParams = httpParams.set(key, params[key]);
    });
    return this.http.get<PaginatedRecords>(`${this.baseUrl}/tables/${tableName}/records`, { params: httpParams });
  }

  getTableStatistics(tableName: string): Observable<TableStatistics> {
    return this.http.get<TableStatistics>(`${this.baseUrl}/tables/${tableName}/statistics`);
  }

  insertRecord(tableName: string, record: RecordData): Observable<any> {
    return this.http.post(`${this.baseUrl}/tables/${tableName}/records`, record);
  }

  updateRecord(tableName: string, id: string, record: Partial<RecordData>): Observable<any> {
    return this.http.patch(`${this.baseUrl}/tables/${tableName}/records/${id}`, record);
  }

  deleteRecord(tableName: string, id: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/tables/${tableName}/records/${id}`);
  }

  importRecords(tableName: string, file: File, strategy: ImportStrategy): Observable<ImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('strategy', strategy);

    return this.http.post<ImportResult>(`${this.baseUrl}/tables/${tableName}/import`, formData);
  }

  exportRecords(tableName: string, params?: any): Observable<Blob> {
    let httpParams = new HttpParams();
    if (params) {
      Object.keys(params).forEach(key => {
        httpParams = httpParams.set(key, params[key]);
      });
    }
    return this.http.get(`${this.baseUrl}/tables/${tableName}/export`, {
      params: httpParams,
      responseType: 'blob'
    });
  }

  searchRecords(tableName: string, query: string, column?: string): Observable<PaginatedRecords> {
    let httpParams = new HttpParams().set('search', query);
    if (column) {
      httpParams = httpParams.set('column', column);
    }
    return this.http.get<PaginatedRecords>(`${this.baseUrl}/tables/${tableName}/search`, { params: httpParams });
  }
}