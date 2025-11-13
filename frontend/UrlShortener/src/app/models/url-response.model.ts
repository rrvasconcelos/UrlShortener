export interface UrlResponse {
  shortCode: string;
  longUrl?: string;
}

export interface CreateShortUrlRequest {
  longUrl: string;
}

export interface CreateShortUrlResponse extends UrlResponse {
  shortCode: string;
}