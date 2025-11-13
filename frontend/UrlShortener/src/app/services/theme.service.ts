import { Injectable } from '@angular/core';

export type Theme = 'light' | 'dark';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly THEME_KEY = 'shorty-theme';
  private currentTheme: Theme;
  
  constructor() {
    this.currentTheme = this.getInitialTheme();
    this.applyTheme(this.currentTheme);
  }
  
  private getInitialTheme(): Theme {
    // 1. Verificar localStorage
    const savedTheme = localStorage.getItem(this.THEME_KEY) as Theme;
    if (savedTheme && (savedTheme === 'light' || savedTheme === 'dark')) {
      return savedTheme;
    }
    
    // 2. Verificar preferência do sistema
    if (typeof window !== 'undefined' && window.matchMedia) {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      return prefersDark ? 'dark' : 'light';
    }
    
    // 3. Padrão: light
    return 'light';
  }
  
  toggleTheme(): void {
    const newTheme: Theme = this.currentTheme === 'light' ? 'dark' : 'light';
    this.setTheme(newTheme);
  }
  
  setTheme(theme: Theme): void {
    this.currentTheme = theme;
    this.applyTheme(theme);
    localStorage.setItem(this.THEME_KEY, theme);
  }
  
  private applyTheme(theme: Theme): void {
    if (typeof document !== 'undefined') {
      const body = document.body;
      
      // Remove classes anteriores
      body.classList.remove('light-theme', 'dark-theme');
      
      // Adiciona nova classe
      body.classList.add(`${theme}-theme`);
      
      // Atualiza meta theme-color para mobile
      const metaTheme = document.querySelector('meta[name="theme-color"]');
      if (metaTheme) {
        metaTheme.setAttribute('content', theme === 'dark' ? '#0f172a' : '#f8fafc');
      }
    }
  }
  
  isDark(): boolean {
    return this.currentTheme === 'dark';
  }
  
  isLight(): boolean {
    return this.currentTheme === 'light';
  }
  
  getCurrentTheme(): Theme {
    return this.currentTheme;
  }
}