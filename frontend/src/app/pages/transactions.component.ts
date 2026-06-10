import { Component, OnInit, OnDestroy } from '@angular/core';
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

          <button class="btn-primary" style="background: var(--accent-color);" (click)="downloadPdf()" [disabled]="isDownloadingPdf">
            {{ isDownloadingPdf ? 'İndiriliyor...' : 'PDF İndir 📄' }}
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

      <!-- Toast Notification -->
      <div class="glass-toast" [class.show]="syncMessage">
        <div class="flex-center" style="gap: 16px;">
          <div class="spinner" *ngIf="isUploading || syncMessage.includes('kuyruğuna') || isSyncing"></div>
          <div class="icon-success" *ngIf="!isUploading && !isSyncing && !syncMessage.includes('kuyruğuna')">✓</div>
          <div>
            <h4 style="margin:0; font-size: 1.05rem; font-weight: 500;">Sistem Bilgisi</h4>
            <p style="margin: 4px 0 0 0; font-size: 0.9rem; color: rgba(255,255,255,0.8);">{{ syncMessage }}</p>
          </div>
        </div>
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
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngIf="isLoading">
              <td colspan="5" class="text-center" style="padding: 32px;">Yükleniyor...</td>
            </tr>
            <tr *ngIf="!isLoading && (!transactions || transactions.items.length === 0)">
              <td colspan="6" class="text-center" style="padding: 32px;">Henüz hiç harcamanız yok.</td>
            </tr>
            <ng-container *ngFor="let t of transactions?.items">
            <tr (click)="t.expanded = !t.expanded" style="cursor: pointer;" [class.row-expanded]="t.expanded">
              <td>
                <div class="flex-center" style="justify-content: flex-start; gap: 12px;">
                  <div class="cat-icon-sm" [style.backgroundColor]="t.categoryColorHex + '20'">
                    <span class="material-icons" style="font-size: 1.2rem;" [style.color]="t.categoryColorHex">{{ t.categoryIcon || 'category' }}</span>
                  </div>
                  <div>
                    <div style="font-weight: 500;">{{ t.title }}</div>
                    <div class="text-muted" style="font-size: 0.85rem;">{{ t.categoryName }}</div>
                  </div>
                </div>
              </td>
              <td class="text-secondary">{{ t.date | date:'dd MMM yyyy, HH:mm' }}</td>
              <td class="text-secondary">
                <div style="max-width: 200px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
                  {{ t.description.split('\n')[0] }}
                </div>
              </td>
              <td>
                <span class="badge-source" [class.badge-auto]="t.isAutomatic">{{ t.source }}</span>
              </td>
              <td style="text-align: right; font-weight: 600;" 
                  [style.color]="t.type === 'Income' ? 'var(--accent-color)' : 'var(--text-primary)'">
                {{ t.type === 'Income' ? '+' : '-' }}₺{{ t.amount | number:'1.2-2' }}
              </td>
              <td style="text-align: right; width: 50px;">
                <button class="btn-delete" (click)="deleteTransaction(t.id); $event.stopPropagation()" title="Sil">
                  🗑️
                </button>
              </td>
            </tr>
            <!-- Expanded Row Details -->
            <tr *ngIf="t.expanded" class="expanded-row-content">
              <td colspan="6" style="padding: 0;">
                <div class="expanded-details flex-between" style="align-items: flex-start; gap: 24px;">
                  <div style="flex: 1;">
                    <h4 class="text-secondary mb-2" style="font-size: 0.9rem; text-transform: uppercase;">İşlem Detayları</h4>
                    <div style="white-space: pre-wrap; font-family: monospace; font-size: 0.95rem; line-height: 1.6; color: var(--text-primary);">{{ t.description }}</div>
                  </div>
                  <div *ngIf="t.receiptImageUrl" class="receipt-image-container" style="width: 200px; flex-shrink: 0;">
                    <h4 class="text-secondary mb-2" style="font-size: 0.8rem; text-transform: uppercase;">Orijinal Fiş</h4>
                    <a [href]="t.receiptImageUrl" target="_blank" title="Büyütmek için tıklayın">
                      <img [src]="t.receiptImageUrl" alt="Fiş Görseli" style="width: 100%; border-radius: 8px; border: 1px solid rgba(255,255,255,0.1); transition: transform 0.2s;" onmouseover="this.style.transform='scale(1.05)'" onmouseout="this.style.transform='scale(1)'"/>
                    </a>
                  </div>
                </div>
              </td>
            </tr>
            </ng-container>
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
    
    .glass-toast {
      position: fixed;
      top: -100px;
      right: 32px;
      background: rgba(15, 23, 42, 0.7);
      backdrop-filter: blur(16px);
      -webkit-backdrop-filter: blur(16px);
      border: 1px solid rgba(255, 255, 255, 0.1);
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
      padding: 16px 24px;
      border-radius: 12px;
      z-index: 9999;
      transition: all 0.4s cubic-bezier(0.175, 0.885, 0.32, 1.275);
      opacity: 0;
      pointer-events: none;
    }
    .glass-toast.show {
      top: 32px;
      opacity: 1;
      pointer-events: auto;
    }
    .spinner {
      width: 24px;
      height: 24px;
      border: 3px solid rgba(255,255,255,0.1);
      border-radius: 50%;
      border-top-color: var(--accent-color);
      animation: spin 1s ease-in-out infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    
    .icon-success {
      width: 28px;
      height: 28px;
      background: rgba(16, 185, 129, 0.2);
      color: #10b981;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
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
    .row-expanded td {
      background: rgba(255,255,255,0.03) !important;
      border-bottom: none !important;
    }
    .expanded-row-content td {
      border-bottom: 1px solid rgba(255,255,255,0.05);
      background: rgba(0,0,0,0.15);
    }
    .expanded-details {
      padding: 20px 24px;
      margin: 8px 24px 24px 24px;
      background: rgba(255,255,255,0.02);
      border-radius: 8px;
      border-left: 3px solid var(--accent-color);
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
    .btn-delete {
      background: transparent;
      border: none;
      cursor: pointer;
      font-size: 1.1rem;
      padding: 6px;
      border-radius: 6px;
      transition: background 0.2s;
      opacity: 0.6;
    }
    .btn-delete:hover {
      background: rgba(239, 68, 68, 0.2);
      opacity: 1;
    }
  `]
})
export class TransactionsComponent implements OnInit, OnDestroy {
  transactions: PaginatedList<TransactionDto> | null = null;
  isLoading = false;
  isSyncing = false;
  isUploading = false;
  isDownloadingPdf = false;
  syncMessage = '';
  currentPage = 1;
  selectedBank = 'Garanti';
  pollingInterval: any;

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

  ngOnDestroy() {
    if (this.pollingInterval) {
      clearInterval(this.pollingInterval);
    }
  }

  loadTransactions(page: number = 1, showLoading: boolean = true) {
    if (showLoading) this.isLoading = true;
    this.currentPage = page;
    this.transactionService.getTransactions(this.currentPage, 10).subscribe({
      next: (data: PaginatedList<TransactionDto>) => {
        this.transactions = data;
        if (showLoading) this.isLoading = false;
      },
      error: (err: any) => {
        console.error('Harcamalar yüklenemedi:', err);
        this.syncMessage = err.error?.message || 'Harcamalar yüklenemedi.';
        if (showLoading) this.isLoading = false;
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
        this.syncMessage = err.error?.message || 'Eşitleme sırasında bir hata oluştu.';
      }
    });
  }

  downloadPdf() {
    this.isDownloadingPdf = true;
    this.syncMessage = 'PDF raporunuz hazırlanıyor...';
    
    this.transactionService.downloadPdf().subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `HarcamaRaporu_${new Date().toISOString().split('T')[0]}.pdf`;
        document.body.appendChild(a);
        a.click();
        
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        
        this.isDownloadingPdf = false;
        this.syncMessage = 'PDF başarıyla indirildi!';
        setTimeout(() => this.syncMessage = '', 3000);
      },
      error: (err: any) => {
        console.error('PDF indirme hatası:', err);
        this.isDownloadingPdf = false;
        this.syncMessage = err.error?.message || 'PDF indirilirken bir hata oluştu.';
        setTimeout(() => this.syncMessage = '', 3000);
      }
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      if (this.pollingInterval) {
        clearInterval(this.pollingInterval);
      }
      this.isUploading = true;
      this.syncMessage = '';
      
      this.transactionService.uploadReceipt(file).subscribe({
        next: () => {
          this.isUploading = false;
          this.syncMessage = 'Fiş başarıyla yapay zeka kuyruğuna alındı! Fiyat taraması yapılıyor...';
          
          // Arka planda tabloyu çaktırmadan her 3 saniyede bir güncelle (maks 30 kere = 90 saniye)
          let attempts = 0;
          this.pollingInterval = setInterval(() => {
            this.loadTransactions(1, false); // Loading ekranı göstermeden yenile!
            attempts++;
            if (attempts >= 30) {
              clearInterval(this.pollingInterval);
              this.syncMessage = ''; // Sadece sessizce kaybolsun
            }
          }, 3000);
        },
        error: (err: any) => {
          console.error('Fiş yükleme hatası:', err);
          this.isUploading = false;
          this.syncMessage = err.error?.message || 'Fiş yüklenirken bir hata oluştu.';
        }
      });
    }
  }

  deleteTransaction(id: string) {
    if (confirm('Bu işlemi silmek istediğinize emin misiniz?')) {
      this.transactionService.deleteTransaction(id).subscribe({
        next: () => {
          this.syncMessage = 'İşlem başarıyla silindi.';
          this.loadTransactions(this.currentPage);
          setTimeout(() => this.syncMessage = '', 3000);
        },
        error: (err: any) => {
          console.error('Silme hatası:', err);
          this.syncMessage = err.error?.message || 'Silme işlemi sırasında bir hata oluştu.';
        }
      });
    }
  }
}
