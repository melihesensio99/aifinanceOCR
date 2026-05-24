import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TransactionDto {
  id: string;
  title: string;
  amount: number;
  type: string;
  date: Date;
  description: string;
  categoryId: string;
  categoryName: string;
  categoryIcon: string;
  categoryColorHex: string;
  isAutomatic: boolean;
  source: string;
}

export interface PaginatedList<T> {
  items: T[];
  pageNumber: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private apiUrl = 'https://localhost:7133/api/transaction';
  private bankApiUrl = 'https://localhost:7133/api/bank';

  constructor(private http: HttpClient) {}

  getTransactions(pageNumber: number = 1, pageSize: number = 10): Observable<PaginatedList<TransactionDto>> {
    return this.http.get<PaginatedList<TransactionDto>>(`${this.apiUrl}?pageNumber=${pageNumber}&pageSize=${pageSize}`);
  }

  syncBank(bankName: string): Observable<any> {
    return this.http.post(`${this.bankApiUrl}/sync`, { bankName });
  }

  uploadReceipt(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post('https://localhost:7133/api/receipt/upload', formData);
  }
}
