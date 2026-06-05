import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-container flex-center">
      <div class="glass-card auth-card">
        <div class="text-center mb-4">
          <h1 class="text-gradient">Hoş Geldiniz</h1>
          <p class="text-secondary">Hesabınıza giriş yapın</p>
        </div>
        
        <form (ngSubmit)="onSubmit()" #loginForm="ngForm">
          <div class="form-group">
            <label class="form-label">E-posta Adresi</label>
            <input type="email" class="form-control" name="email" [(ngModel)]="credentials.email" required placeholder="ornek@mail.com">
          </div>
          
          <div class="form-group">
            <label class="form-label">Şifre</label>
            <input type="password" class="form-control" name="password" [(ngModel)]="credentials.password" required placeholder="••••••••">
          </div>
          
          <div *ngIf="errorMessage" class="error-msg">
            {{ errorMessage }}
          </div>
          
          <button type="submit" class="btn-primary w-100" [disabled]="!loginForm.form.valid || isLoading">
            {{ isLoading ? 'Giriş Yapılıyor...' : 'Giriş Yap' }}
          </button>
        </form>
        
        <div class="text-center mt-4">
          <p class="text-muted">
            Hesabınız yok mu? <a routerLink="/register" class="text-gradient-primary">Kayıt Olun</a>
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: calc(100vh - 200px);
    }
    .auth-card {
      width: 100%;
      max-width: 420px;
    }
    .text-center { text-align: center; }
    .mb-4 { margin-bottom: 24px; }
    .mt-4 { margin-top: 24px; }
    .w-100 { width: 100%; justify-content: center; }
    .error-msg {
      color: var(--danger-color);
      background: rgba(244, 63, 94, 0.1);
      padding: 12px;
      border-radius: 8px;
      margin-bottom: 16px;
      font-size: 0.875rem;
      text-align: center;
    }
  `]
})
export class LoginComponent {
  credentials = { email: '', password: '' };
  isLoading = false;
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.login(this.credentials).subscribe({
      next: () => {
        this.router.navigate(['/']); // Dashboard'a yönlendir
      },
      error: (err: any) => {
        this.isLoading = false;
        
        // Backend'den gelen hata mesajını yakala (FluentValidation mesajı veya genel hata)
        if (err.error && err.error.message) {
            this.errorMessage = err.error.message;
        } else {
            this.errorMessage = 'Giriş başarısız. Lütfen bilgilerinizi kontrol edin.';
        }
        
        console.error(err);
      }
    });
  }
}
