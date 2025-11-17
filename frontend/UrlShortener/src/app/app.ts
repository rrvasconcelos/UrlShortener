import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { UrlShortenerComponent } from './components/url-shortener.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, UrlShortenerComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('UrlShortener');
}
