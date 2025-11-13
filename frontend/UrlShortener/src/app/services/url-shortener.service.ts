import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CreateShortUrlRequest, CreateShortUrlResponse } from '../models/url-response.model';

@Injectable({
  providedIn: 'root'
})
export class UrlShortenerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl || 'https://localhost:7000';

  createShortUrl(request: CreateShortUrlRequest): Observable<CreateShortUrlResponse> {
    return this.http.post<CreateShortUrlResponse>(`${this.baseUrl}/v1/shorten`, request);
  }

  getFullUrl(shortCode: string): string {
    return `${this.baseUrl}/v1/shorten/${shortCode}`;
  }
}