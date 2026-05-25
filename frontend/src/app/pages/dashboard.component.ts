import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AnalyticsService, DashboardSummaryDto } from '../core/services/analytics.service';
import { AuthService } from '../core/services/auth.service';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

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
          <p class="text-secondary">Tüm harcama analizleriniz</p>
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
          <div class="glass-card" style="display: flex; flex-direction: column; align-items: center; justify-content: center; padding: 40px;">
            <h3 class="text-secondary" style="font-size: 1.2rem; margin-bottom: 12px;">{{ selectedPeriodLabel }} Toplam Harcama</h3>
            <h2 class="text-gradient-primary" style="font-size: 3.5rem; margin-bottom: 0;">₺{{ summaryData.totalExpense | number:'1.2-2' }}</h2>
          </div>
        </div>

        <!-- Categories Section -->
        <div class="categories-section mt-4">
          <h2 class="text-gradient mb-4">Kategorilere Göre Dağılım</h2>
          
          <div *ngIf="summaryData.categorySummaries.length === 0" class="glass-card text-center">
            <p class="text-muted">Bu dönemde henüz hiçbir harcama kaydınız bulunmuyor.</p>
          </div>

          <div class="glass-card" *ngIf="summaryData.categorySummaries.length > 0">
            <div class="chart-layout">
              <!-- Left side: Donut Chart -->
              <div class="chart-container">
                <canvas #categoryChart></canvas>
              </div>

              <!-- Right side: Legend / List -->
              <div class="category-list">
                <div *ngFor="let cat of summaryData.categorySummaries" class="category-item">
                  <div class="category-header flex-between">
                    <div class="flex-center" style="gap: 16px;">
                      <div class="cat-icon" [style.backgroundColor]="cat.categoryColorHex + '20'">
                        <span class="material-icons" [style.color]="cat.categoryColorHex">{{ cat.categoryIcon || 'category' }}</span>
                      </div>
                      <span class="cat-name">{{ cat.categoryName }}</span>
                    </div>
                    <div class="cat-amount">
                      ₺{{ cat.totalAmount | number:'1.2-2' }} 
                      <span class="text-muted" style="font-size: 0.9rem; margin-left: 8px;">({{ cat.percentage | number:'1.1-1' }}%)</span>
                    </div>
                  </div>
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
      margin-bottom: 32px;
    }
    
    .chart-layout {
      display: flex;
      flex-wrap: wrap;
      gap: 40px;
      align-items: center;
      justify-content: center;
    }
    .chart-container {
      flex: 1;
      min-width: 250px;
      max-width: 350px;
      position: relative;
    }
    .category-list {
      flex: 1.5;
      min-width: 300px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }
    .category-item {
      padding: 16px 20px;
      background: rgba(255,255,255,0.02);
      border: 1px solid rgba(255,255,255,0.05);
      border-radius: 16px;
      transition: all 0.3s ease;
    }
    .category-item:hover {
      background: rgba(255,255,255,0.05);
      transform: translateX(5px);
      border-color: rgba(255,255,255,0.1);
    }
    
    .cat-icon {
      width: 48px;
      height: 48px;
      border-radius: 14px;
      display: flex;
      justify-content: center;
      align-items: center;
      font-size: 1.5rem;
    }
    .cat-name { font-weight: 500; font-size: 1.2rem; }
    .cat-amount { font-weight: 600; font-size: 1.2rem; }
    
    .mb-4 { margin-bottom: 24px; }
    .mt-4 { margin-top: 32px; }
    .text-center { text-align: center; }
  `]
})
export class DashboardComponent implements OnInit {
  @ViewChild('categoryChart') chartCanvas!: ElementRef<HTMLCanvasElement>;
  
  Math = Math;
  summaryData: DashboardSummaryDto | null = null;
  isLoading = false;
  selectedPeriod: string = 'Monthly';
  chartInstance: Chart | null = null;

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
        
        if (data && data.categorySummaries.length > 0) {
          setTimeout(() => this.renderChart(), 50);
        }
      },
      error: (err: any) => {
        console.error('Veriler yüklenirken hata oluştu:', err);
        this.isLoading = false;
      }
    });
  }

  renderChart() {
    if (!this.summaryData || !this.chartCanvas) return;
    
    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    if (this.chartInstance) {
      this.chartInstance.destroy();
    }

    const labels = this.summaryData.categorySummaries.map(c => c.categoryName);
    const data = this.summaryData.categorySummaries.map(c => c.totalAmount);
    const bgColors = this.summaryData.categorySummaries.map(c => c.categoryColorHex || '#808080');

    this.chartInstance = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: labels,
        datasets: [{
          data: data,
          backgroundColor: bgColors,
          borderWidth: 0,
          hoverOffset: 10
        }]
      },
      options: {
        responsive: true,
        cutout: '75%',
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            padding: 12,
            titleFont: { size: 14 },
            bodyFont: { size: 14 },
            callbacks: {
              label: (context) => {
                let value = context.raw as number;
                return ' ' + context.label + ': ₺' + value.toFixed(2);
              }
            }
          }
        },
        animation: {
          animateScale: true,
          animateRotate: true
        }
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
