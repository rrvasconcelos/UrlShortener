import { Component, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Clipboard } from '@angular/cdk/clipboard';
import { UrlShortenerService } from '../services/url-shortener.service';
import { ThemeService } from '../services/theme.service';
import { CreateShortUrlResponse } from '../models/url-response.model';

@Component({
  selector: 'app-url-shortener',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDividerModule,
    MatTooltipModule
  ],
  template: `
    <div class="container" [class.dark-mode]="themeService.isDark()">
      <!-- Theme Toggle -->
      <div class="theme-toggle-wrapper">
        <button 
          class="theme-toggle-btn"
          (click)="themeService.toggleTheme()"
          [attr.aria-label]="themeService.isDark() ? 'Ativar modo claro' : 'Ativar modo escuro'"
          matTooltip="{{ themeService.isDark() ? 'Modo Claro' : 'Modo Escuro' }}"
          matTooltipPosition="left"
        >
          <div class="toggle-track">
            <div class="toggle-thumb" [class.dark]="themeService.isDark()">
              <mat-icon class="toggle-icon sun-icon" [class.active]="themeService.isLight()">
                light_mode
              </mat-icon>
              <mat-icon class="toggle-icon moon-icon" [class.active]="themeService.isDark()">
                dark_mode
              </mat-icon>
            </div>
          </div>
        </button>
      </div>

      <!-- Background decorations -->
      <div class="bg-decoration">
        <div class="blob blob-1"></div>
        <div class="blob blob-2"></div>
        <div class="blob blob-3"></div>
      </div>

      <!-- Header Hero Section -->
      <div class="hero-header">
        <div class="hero-content">
          <div class="hero-logo">
            <div class="logo-container">
              <div class="logo-icon">
                <mat-icon class="link-icon">link</mat-icon>
                <div class="cut-effect">
                  <div class="cut-line"></div>
                  <div class="cut-line"></div>
                </div>
              </div>
              <div class="logo-text">
                <span class="logo-s">S</span>
              </div>
            </div>
          </div>
          <h1 class="hero-title">
            <span class="gradient-text">Shorty</span>
            <span class="subtitle-text">.com</span>
          </h1>
          <p class="hero-description">
            O encurtador de URLs mais rápido e confiável. Transforme links longos em URLs curtas e poderosas.
          </p>
          <div class="hero-stats">
            <div class="stat-item">
              <div class="stat-icon">
                <mat-icon>all_inclusive</mat-icon>
              </div>
              <span class="stat-label">URLs Processadas</span>
            </div>
            <div class="stat-item">
              <div class="stat-icon lightning">
                <mat-icon>flash_on</mat-icon>
              </div>
              <span class="stat-label">Super Rápido</span>
            </div>
            <div class="stat-item">
              <div class="stat-icon security">
                <mat-icon>security</mat-icon>
              </div>
              <span class="stat-label">100% Seguro</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Main Card -->
      <div class="main-card">
        <div class="card-glow"></div>
        <div class="card-content">
          <div class="input-section">
            <h3 class="section-title">Cole sua URL aqui</h3>
            <form [formGroup]="urlForm" (ngSubmit)="shortenUrl()" class="url-form">
              <div class="input-wrapper">
                <mat-form-field appearance="outline" class="url-input">
                  <mat-label>URL para encurtar</mat-label>
                  <input
                    matInput
                    formControlName="longUrl"
                    placeholder="https://exemplo.com/minha-url-longa"
                    type="url"
                    [disabled]="isLoading()"
                    class="modern-input"
                  >
                  <mat-icon matSuffix class="input-icon">insert_link</mat-icon>
                  @if (urlForm.get('longUrl')?.hasError('required') && urlForm.get('longUrl')?.touched) {
                    <mat-error>✨ Cole uma URL para começar a magia</mat-error>
                  }
                  @if (urlForm.get('longUrl')?.hasError('pattern') && urlForm.get('longUrl')?.touched) {
                    <mat-error>🔗 Esta URL precisa de http:// ou https://</mat-error>
                  }
                </mat-form-field>
                
                <button
                  mat-fab
                  color="primary"
                  type="submit"
                  [disabled]="urlForm.invalid || isLoading()"
                  class="magic-button"
                  matTooltip="Encurtar URL"
                >
                  @if (isLoading()) {
                    <mat-spinner diameter="24" class="button-spinner"></mat-spinner>
                  } @else {
                    <mat-icon>auto_fix_high</mat-icon>
                  }
                </button>
              </div>
            </form>
          </div>

          @if (shortenedUrl()) {
            <div class="result-section">
              <div class="success-header">
                <div class="success-icon">
                  <mat-icon>celebration</mat-icon>
                </div>
                <h3 class="success-title">Pronto! Sua URL foi encurtada</h3>
                <p class="success-subtitle">Agora você pode compartilhar facilmente</p>
              </div>

              <div class="result-container">
                <div class="short-url-display">
                  <div class="url-preview">
                    <mat-icon class="preview-icon">link</mat-icon>
                    <span class="short-url-text">{{ shortenedUrl() }}</span>
                  </div>
                  
                  <div class="url-actions">
                    <button
                      mat-mini-fab
                      color="primary"
                      matTooltip="Copiar URL"
                      (click)="copyToClipboard(shortenedUrl()!)"
                      class="action-btn copy-btn"
                    >
                      <mat-icon>content_copy</mat-icon>
                    </button>
                    
                    <button
                      mat-mini-fab
                      color="accent"
                      matTooltip="Testar URL"
                      (click)="openUrl(shortenedUrl()!)"
                      class="action-btn test-btn"
                    >
                      <mat-icon>open_in_new</mat-icon>
                    </button>
                  </div>
                </div>

                <button
                  mat-stroked-button
                  (click)="reset()"
                  class="new-url-btn"
                >
                  <mat-icon>refresh</mat-icon>
                  Encurtar Nova URL
                </button>
              </div>
            </div>
          }

          @if (error()) {
            <div class="error-section">
              <div class="error-content">
                <mat-icon class="error-icon">sentiment_dissatisfied</mat-icon>
                <h4 class="error-title">Oops! Algo deu errado</h4>
                <p class="error-message">{{ error() }}</p>
                <button mat-button color="primary" (click)="reset()">
                  Tentar Novamente
                </button>
              </div>
            </div>
          }
        </div>
      </div>

      <!-- Features Section -->
      <div class="features-section">
        <div class="feature-card">
          <mat-icon>flash_on</mat-icon>
          <h4>Ultra Rápido</h4>
          <p>URLs encurtadas em millisegundos</p>
        </div>
        <div class="feature-card">
          <mat-icon>security</mat-icon>
          <h4>Seguro</h4>
          <p>Seus links estão protegidos</p>
        </div>
        <div class="feature-card">
          <mat-icon>share</mat-icon>
          <h4>Fácil Compartilhar</h4>
          <p>Copie e cole onde quiser</p>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./url-shortener.component.scss']
})
export class UrlShortenerComponent {
  private readonly fb = inject(FormBuilder);
  private readonly urlService = inject(UrlShortenerService);
  readonly themeService = inject(ThemeService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly clipboard = inject(Clipboard);

  // Signals
  readonly isLoading = signal(false);
  readonly shortenedUrl = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  // Form
  readonly urlForm: FormGroup = this.fb.group({
    longUrl: ['', [
      Validators.required,
      Validators.pattern(/^https?:\/\/[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?(\.[a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?)*(\:[0-9]{1,5})?(\/[^\s]*)?$/)
    ]]
  });

  shortenUrl(): void {
    if (this.urlForm.invalid) {
      this.urlForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    this.error.set(null);
    this.shortenedUrl.set(null);

    const longUrl = this.urlForm.get('longUrl')?.value;

    this.urlService.createShortUrl({ LongUrl: longUrl }).subscribe({
      next: (response: CreateShortUrlResponse) => {
        const fullShortUrl = this.urlService.getFullUrl(response.shortCode);
        this.shortenedUrl.set(fullShortUrl);
        this.isLoading.set(false);
        this.showSuccessMessage();
      },
      error: (error) => {
        console.error('Erro ao encurtar URL:', error);
        this.error.set('Erro ao encurtar a URL. Tente novamente.');
        this.isLoading.set(false);
      }
    });
  }

  copyToClipboard(url: string): void {
    if (this.clipboard.copy(url)) {
      this.snackBar.open('URL copiada para a área de transferência!', 'Fechar', {
        duration: 3000,
        horizontalPosition: 'center',
        verticalPosition: 'bottom'
      });
    }
  }

  openUrl(url: string): void {
    window.open(url, '_blank');
  }

  reset(): void {
    this.urlForm.reset();
    this.shortenedUrl.set(null);
    this.error.set(null);
  }

  private showSuccessMessage(): void {
    this.snackBar.open('URL encurtada com sucesso!', 'Fechar', {
      duration: 3000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom'
    });
  }
}