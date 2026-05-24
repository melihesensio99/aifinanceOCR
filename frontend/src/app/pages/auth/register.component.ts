import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="auth-container flex-center">
      <div class="glass-card auth-card">
        <div class="text-center mb-4">
          <h1 class="text-gradient">Kayıt Olun</h1>
          <p class="text-secondary">Finansal yolculuğunuza başlayın</p>
        </div>
        
        <form (ngSubmit)="onSubmit()" #registerForm="ngForm">
          <div class="flex-between" style="gap: 16px;">
            <div class="form-group" style="flex: 1;">
              <label class="form-label">Ad</label>
              <input type="text" class="form-control" name="firstName" [(ngModel)]="userData.firstName" required placeholder="Adınız">
            </div>
            <div class="form-group" style="flex: 1;">
              <label class="form-label">Soyad</label>
              <input type="text" class="form-control" name="lastName" [(ngModel)]="userData.lastName" required placeholder="Soyadınız">
            </div>
          </div>

          <div class="form-group">
            <label class="form-label">E-posta Adresi</label>
            <input type="email" class="form-control" name="email" [(ngModel)]="userData.email" required placeholder="ornek@mail.com">
          </div>
          
          <div class="form-group">
            <label class="form-label">Şifre</label>
            <input type="password" class="form-control" name="password" [(ngModel)]="userData.password" required placeholder="••••••••">
          </div>
          
          <div *ngIf="errorMessage" class="error-msg">
            {{ errorMessage }}
          </div>
          
          <button type="submit" class="btn-primary w-100" [disabled]="!registerForm.form.valid || isLoading">
            {{ isLoading ? 'Hesap Oluşturuluyor...' : 'Hesap Oluştur' }}
          </button>
        </form>
        
        <div class="text-center mt-4">
          <p class="text-muted">
            Zaten hesabınız var mı? <a routerLink="/login" class="text-gradient-primary">Giriş Yapın</a>
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
      max-width: 480px;
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
export class RegisterComponent {
  userData = { firstName: '', lastName: '', email: '', password: '' };
  isLoading = false;
  errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.isLoading = true;
    this.errorMessage = '';
    
    this.authService.register(this.userData).subscribe({
      next: () => {
        // Kayıt başarılıysa Login sayfasına gönder
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'Kayıt başarısız. Bu e-posta kullanılıyor olabilir veya şifre kurallara uymuyor.';
        console.error(err);
      }
    });
  }
}
