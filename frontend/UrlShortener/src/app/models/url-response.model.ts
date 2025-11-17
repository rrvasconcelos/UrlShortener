export interface UrlResponse {
  shortCode: string;
  longUrl?: string;
}

export interface CreateShortUrlRequest {
  LongUrl: string;
}

export interface CreateShortUrlResponse extends UrlResponse {
  shortCode: string;
}