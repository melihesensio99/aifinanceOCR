import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CategorySummaryDto {
  categoryId: string;
  categoryName: string;
  categoryIcon: string;
  categoryColorHex: string;
  totalAmount: number;
  percentage: number;
}

export interface DashboardSummaryDto {
  totalExpense: number;
  totalIncome: number;
  netBalance: number;
  transactionCount: number;
  startDate: Date;
  endDate: Date;
  categorySummaries: CategorySummaryDto[];
}

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private apiUrl = 'https://localhost:7133/api/analytics';

  constructor(private http: HttpClient) {}

  getDashboardSummary(period: string = 'AllTime'): Observable<DashboardSummaryDto> {
    return this.http.get<DashboardSummaryDto>(`${this.apiUrl}/dashboard?period=${period}`);
  }
}
