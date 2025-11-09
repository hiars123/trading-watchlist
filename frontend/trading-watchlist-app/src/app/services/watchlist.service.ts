// frontend/src/app/services/watchlist.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Stock {
  id: number;
  ticker: string;
  addedAt: Date;
  source?: string;
  notes?: string;
  currentPrice?: number;
  priceUpdatedAt?: Date;
  alerts: Alert[];
  screenshotCount: number;
}

export interface Alert {
  id: number;
  targetPrice: number;
  isTriggered: boolean;
  distancePercent?: number;
}

export interface CreateStock {
  ticker: string;
  source?: string;
  initialAlertPrice?: number;
}

export interface CreateAlert {
  stockId: number;
  targetPrice: number;
}

@Injectable({
  providedIn: 'root'
})
export class WatchlistService {
  private apiUrl = `${environment.apiUrl}/api`;

  constructor(private http: HttpClient) { }

  // Watchlist endpoints
  getAllStocks(): Observable<Stock[]> {
    return this.http.get<Stock[]>(`${this.apiUrl}/watchlist`);
  }

  getStockById(id: number): Observable<Stock> {
    return this.http.get<Stock>(`${this.apiUrl}/watchlist/${id}`);
  }

  addStock(stock: CreateStock): Observable<Stock> {
    return this.http.post<Stock>(`${this.apiUrl}/watchlist`, stock);
  }

  removeStock(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/watchlist/${id}`);
  }

  updateNotes(id: number, notes: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/watchlist/${id}/notes`, JSON.stringify(notes), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  updateSource(id: number, source: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/watchlist/${id}/source`, JSON.stringify(source), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  // Alert endpoints
  getAlertsForStock(stockId: number): Observable<Alert[]> {
    return this.http.get<Alert[]>(`${this.apiUrl}/alerts/stock/${stockId}`);
  }

  createAlert(alert: CreateAlert): Observable<Alert> {
    return this.http.post<Alert>(`${this.apiUrl}/alerts`, alert);
  }

  deleteAlert(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/alerts/${id}`);
  }

  // Price endpoints
  getCurrentPrice(ticker: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/prices/${ticker}`);
  }
}