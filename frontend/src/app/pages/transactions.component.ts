import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransactionService, TransactionDto, PaginatedList } from '../core/services/transaction.service';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div *ngIf="!(authService.isLoggedIn$ | async)" class="flex-center" style="min-height: 50vh;">
      <div class="glass-card text-center" style="max-width: 400px;">
        <h2 class="text-gradient">Hoş Geldiniz</h2>
        <p class="text-secondary" style="margin-bottom: 24px;">Harcamalarınızı görmek için lütfen giriş yapın.</p>
        <a href="/login" class="btn-primary">Giriş Yap</a>
      </div>
    </div>

    <div *ngIf="authService.isLoggedIn$ | async">
      <div class="dashboard-header flex-between">
        <div>
          <h1 class="text-gradient">Harcamalarınız</h1>
          <p class="text-secondary">Tüm finansal hareketlerinizin detaylı listesi</p>
        </div>
        
        <div class="filter-group flex-center" style="gap: 16px;">
          <!-- Gizli dosya seçici -->
          <input type="file" #fileInput (change)="onFileSelected($event)" style="display: none" accept="image/*">
          <button class="btn-primary" style="background: var(--electric-violet);" (click)="fileInput.click()" [disabled]="isUploading">
            📸 {{ isUploading ? 'Yükleniyor...' : 'Fiş Okut' }}
          </button>

          <select class="form-control" [(ngModel)]="selectedBank" style="min-width: 150px;">
            <option value="Garanti">Garanti BBVA</option>
            <option value="Akbank">Akbank</option>
            <option value="Ziraat">Ziraat Bankası</option>
          </select>
          <button class="btn-primary" (click)="syncBank()" [disabled]="isSyncing">
            {{ isSyncing ? 'Eşitleniyor...' : 'Banka Eşitle' }}
          </button>
        </div>
      </div>

      <div *ngIf="syncMessage" class="success-msg mb-4">
        {{ syncMessage }}
      </div>

      <div class="glass-card" style="padding: 0; overflow: hidden;">
        <table class="transaction-table w-100">
          <thead>
            <tr>
              <th>İşlem / Kategori</th>
              <th>Tarih</th>
              <th>Açıklama</th>
              <th>Kaynak</th>
              <th style="text-align: right;">Tutar</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngIf="isLoading">
              <td colspan="5" class="text-center" style="padding: 32px;">Yükleniyor...</td>
            </tr>
            <tr *ngIf="!isLoading && (!transactions || transactions.items.length === 0)">
              <td colspan="5" class="text-center" style="padding: 32px;">Henüz hiç harcamanız yok.</td>
            </tr>
            <tr *ngFor="let t of transactions?.items">
              <td>
                <div class="flex-center" style="justify-content: flex-start; gap: 12px;">
                  <div class="cat-icon-sm" [style.backgroundColor]="t.categoryColorHex + '20'">
                    <span [style.color]="t.categoryColorHex">{{ t.categoryIcon || '📌' }}</span>
                  </div>
                  <div>
                    <div style="font-weight: 500;">{{ t.title }}</div>
                    <div class="text-muted" style="font-size: 0.85rem;">{{ t.categoryName }}</div>
                  </div>
                </div>
              </td>
              <td class="text-secondary">{{ t.date | date:'dd MMM yyyy, HH:mm' }}</td>
              <td class="text-secondary">{{ t.description }}</td>
              <td>
                <span class="badge-source" [class.badge-auto]="t.isAutomatic">{{ t.source }}</span>
              </td>
              <td style="text-align: right; font-weight: 600;" 
                  [style.color]="t.type === 'Income' ? 'var(--accent-color)' : 'var(--text-primary)'">
                {{ t.type === 'Income' ? '+' : '-' }}₺{{ t.amount | number:'1.2-2' }}
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Pagination -->
        <div class="pagination flex-between" *ngIf="transactions && transactions.totalPages > 1">
          <button class="btn-outline" style="padding: 8px 16px;" 
                  (click)="changePage(currentPage - 1)" 
                  [disabled]="!transactions.hasPreviousPage">
            Önceki
          </button>
          
          <span class="text-secondary">
            Sayfa {{ transactions.pageNumber }} / {{ transactions.totalPages }} 
            (Toplam: {{ transactions.totalCount }})
          </span>
          
          <button class="btn-outline" style="padding: 8px 16px;" 
                  (click)="changePage(currentPage + 1)" 
                  [disabled]="!transactions.hasNextPage">
            Sonraki
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-header { margin-bottom: 32px; }
    .w-100 { width: 100%; }
    .mb-4 { margin-bottom: 24px; }
    .text-center { text-align: center; }
    
    .success-msg {
      color: var(--accent-color);
      background: rgba(16, 185, 129, 0.1);
      padding: 16px;
      border-radius: 8px;
      border: 1px solid rgba(16, 185, 129, 0.3);
    }
    
    .transaction-table {
      border-collapse: collapse;
      width: 100%;
    }
    .transaction-table th, .transaction-table td {
      padding: 16px 24px;
      text-align: left;
      border-bottom: 1px solid rgba(255,255,255,0.05);
    }
    .transaction-table th {
      color: var(--text-muted);
      font-weight: 500;
      font-size: 0.85rem;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      background: rgba(0,0,0,0.2);
    }
    .transaction-table tr:hover td {
      background: rgba(255,255,255,0.02);
    }
    
    .cat-icon-sm {
      width: 32px;
      height: 32px;
      border-radius: 8px;
      display: flex;
      justify-content: center;
      align-items: center;
      font-size: 1rem;
    }
    
    .badge-source {
      background: rgba(255,255,255,0.1);
      color: var(--text-secondary);
      padding: 4px 10px;
      border-radius: 6px;
      font-size: 0.75rem;
    }
    .badge-auto {
      background: rgba(99, 102, 241, 0.15);
      color: var(--primary-color);
      border: 1px solid rgba(99, 102, 241, 0.3);
    }
    
    .pagination {
      padding: 16px 24px;
      background: rgba(0,0,0,0.1);
      border-top: 1px solid rgba(255,255,255,0.05);
    }
  `]
})
export class TransactionsComponent implements OnInit {
  transactions: PaginatedList<TransactionDto> | null = null;
  isLoading = false;
  isSyncing = false;
  isUploading = false;
  syncMessage = '';
  currentPage = 1;
  selectedBank = 'Garanti';

  constructor(
    public authService: AuthService,
    private transactionService: TransactionService
  ) {}

  ngOnInit() {
    this.authService.isLoggedIn$.subscribe((isLoggedIn: boolean) => {
      if (isLoggedIn) {
        this.loadTransactions();
      }
    });
  }

  loadTransactions(page: number = 1) {
    this.isLoading = true;
    this.currentPage = page;
    this.transactionService.getTransactions(this.currentPage, 10).subscribe({
      next: (data: PaginatedList<TransactionDto>) => {
        this.transactions = data;
        this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Harcamalar yüklenemedi:', err);
        this.isLoading = false;
      }
    });
  }

  changePage(page: number) {
    if (page >= 1 && (!this.transactions || page <= this.transactions.totalPages)) {
      this.loadTransactions(page);
    }
  }

  syncBank() {
    this.isSyncing = true;
    this.syncMessage = '';
    
    this.transactionService.syncBank(this.selectedBank).subscribe({
      next: () => {
        this.isSyncing = false;
        this.syncMessage = `${this.selectedBank} hesap hareketleri başarıyla eşitlendi!`;
        this.loadTransactions(1);
        setTimeout(() => this.syncMessage = '', 5000);
      },
      error: (err: any) => {
        console.error('Banka eşitleme hatası:', err);
        this.isSyncing = false;
        this.syncMessage = 'Eşitleme sırasında bir hata oluştu.';
      }
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.isUploading = true;
      this.syncMessage = '';
      
      this.transactionService.uploadReceipt(file).subscribe({
        next: () => {
          this.isUploading = false;
          this.syncMessage = 'Fiş başarıyla yapay zeka kuyruğuna alındı! 15 sn içinde tabloya yansıyacaktır.';
          // OCR asenkron (RabbitMQ) çalıştığı için tablo hemen yenilenmez. 
          // Gerçekte SignalR/WebSocket ile dinlenir ama biz basitçe 10sn sonra sayfayı yeniliyoruz.
          setTimeout(() => this.loadTransactions(1), 10000);
          setTimeout(() => this.syncMessage = '', 10000);
        },
        error: (err: any) => {
          console.error('Fiş yükleme hatası:', err);
          this.isUploading = false;
          this.syncMessage = 'Fiş yüklenirken bir hata oluştu.';
        }
      });
    }
  }
}
