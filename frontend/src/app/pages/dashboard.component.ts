import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService, DashboardSummaryDto } from '../core/services/analytics.service';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div *ngIf="!(authService.isLoggedIn$ | async)" class="flex-center" style="min-height: 50vh;">
      <div class="glass-card text-center" style="max-width: 400px;">
        <h2 class="text-gradient">Hoş Geldiniz</h2>
        <p class="text-secondary" style="margin-bottom: 24px;">Finansal özetinizi görmek için lütfen giriş yapın.</p>
        <a href="/login" class="btn-primary">Giriş Yap</a>
      </div>
    </div>

    <div *ngIf="authService.isLoggedIn$ | async">
      <div class="dashboard-header flex-between">
        <div>
          <h1 class="text-gradient">Finansal Özetiniz</h1>
          <p class="text-secondary">Tüm harcama ve gelirlerinizin analizi</p>
        </div>
        
        <div class="filter-group">
          <select class="form-control" [(ngModel)]="selectedPeriod" (change)="loadDashboardData()">
            <option value="Weekly">Bu Hafta</option>
            <option value="Monthly">Bu Ay</option>
            <option value="Yearly">Bu Yıl</option>
            <option value="AllTime">Tüm Zamanlar</option>
          </select>
        </div>
      </div>

      <div *ngIf="isLoading" class="flex-center" style="padding: 40px;">
        <p class="text-muted">Verileriniz yükleniyor...</p>
      </div>

      <ng-container *ngIf="!isLoading && summaryData">
        <!-- Top Cards -->
        <div class="summary-cards">
          <div class="glass-card">
            <h3 class="text-secondary">Toplam Harcama</h3>
            <h2 class="text-gradient-primary">₺{{ summaryData.totalExpense | number:'1.2-2' }}</h2>
            <span class="badge-expense">{{ selectedPeriodLabel }}</span>
          </div>
          
          <div class="glass-card">
            <h3 class="text-secondary">Toplam Gelir</h3>
            <h2 style="color: var(--accent-color)">₺{{ summaryData.totalIncome | number:'1.2-2' }}</h2>
            <span class="badge-income">{{ selectedPeriodLabel }}</span>
          </div>

          <div class="glass-card">
            <h3 class="text-secondary">Net Bakiye</h3>
            <h2 [style.color]="summaryData.netBalance >= 0 ? 'var(--accent-color)' : 'var(--danger-color)'">
              ₺{{ Math.abs(summaryData.netBalance) | number:'1.2-2' }}
            </h2>
            <span [class]="summaryData.netBalance >= 0 ? 'badge-income' : 'badge-expense'">
              {{ summaryData.netBalance >= 0 ? 'Kâr' : 'Zarar' }}
            </span>
          </div>
        </div>

        <!-- Categories Section -->
        <div class="categories-section mt-4">
          <h2 class="text-gradient mb-4">Kategorilere Göre Harcamalar</h2>
          
          <div *ngIf="summaryData.categorySummaries.length === 0" class="glass-card text-center">
            <p class="text-muted">Bu dönemde henüz hiçbir harcama kaydınız bulunmuyor.</p>
          </div>

          <div class="glass-card" *ngIf="summaryData.categorySummaries.length > 0">
            <div *ngFor="let cat of summaryData.categorySummaries" class="category-item">
              <div class="category-header flex-between">
                <div class="flex-center" style="gap: 12px;">
                  <div class="cat-icon" [style.backgroundColor]="cat.categoryColorHex + '20'">
                    <span [style.color]="cat.categoryColorHex">{{ cat.categoryIcon || '📌' }}</span>
                  </div>
                  <span class="cat-name">{{ cat.categoryName }}</span>
                </div>
                <div class="cat-amount">
                  ₺{{ cat.totalAmount | number:'1.2-2' }} 
                  <span class="text-muted" style="font-size: 0.85rem;">({{ cat.percentage | number:'1.1-1' }}%)</span>
                </div>
              </div>
              
              <!-- Progress Bar -->
              <div class="progress-track">
                <div class="progress-fill" 
                     [style.width]="cat.percentage + '%'"
                     [style.backgroundColor]="cat.categoryColorHex || 'var(--primary-color)'">
                </div>
              </div>
            </div>
          </div>
        </div>
      </ng-container>
    </div>
  `,
  styles: [`
    .dashboard-header { margin-bottom: 40px; }
    .filter-group select { min-width: 150px; }
    
    .summary-cards {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 24px;
    }
    .glass-card h3 { font-size: 1rem; font-weight: 500; margin-bottom: 8px; }
    .glass-card h2 { font-size: 2.2rem; margin-bottom: 16px; }
    
    .category-item {
      margin-bottom: 24px;
    }
    .category-item:last-child {
      margin-bottom: 0;
    }
    
    .cat-icon {
      width: 40px;
      height: 40px;
      border-radius: 12px;
      display: flex;
      justify-content: center;
      align-items: center;
      font-size: 1.2rem;
    }
    .cat-name { font-weight: 600; font-size: 1.1rem; }
    .cat-amount { font-weight: 600; font-size: 1.1rem; }
    
    .progress-track {
      width: 100%;
      height: 8px;
      background: rgba(255,255,255,0.05);
      border-radius: 4px;
      margin-top: 12px;
      overflow: hidden;
    }
    .progress-fill {
      height: 100%;
      border-radius: 4px;
      transition: width 1s ease-in-out;
    }
    .mb-4 { margin-bottom: 24px; }
    .mt-4 { margin-top: 32px; }
    .text-center { text-align: center; }
  `]
})
export class DashboardComponent implements OnInit {
  Math = Math;
  summaryData: DashboardSummaryDto | null = null;
  isLoading = false;
  selectedPeriod: string = 'Monthly';

  constructor(
    public authService: AuthService,
    private analyticsService: AnalyticsService
  ) {}

  ngOnInit() {
    this.authService.isLoggedIn$.subscribe((isLoggedIn: boolean) => {
      if (isLoggedIn) {
        this.loadDashboardData();
      }
    });
  }

  loadDashboardData() {
    this.isLoading = true;
    this.analyticsService.getDashboardSummary(this.selectedPeriod).subscribe({
      next: (data: DashboardSummaryDto) => {
        this.summaryData = data;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Veriler yüklenirken hata oluştu:', err);
        this.isLoading = false;
      }
    });
  }

  get selectedPeriodLabel(): string {
    switch(this.selectedPeriod) {
      case 'Weekly': return 'Bu Hafta';
      case 'Monthly': return 'Bu Ay';
      case 'Yearly': return 'Bu Yıl';
      case 'AllTime': return 'Tüm Zamanlar';
      default: return '';
    }
  }
}
