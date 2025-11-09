// frontend/src/app/components/watchlist-table/watchlist-table.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { WatchlistService, Stock, CreateStock } from '../../services/watchlist.service';

@Component({
  selector: 'app-watchlist-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule
  ],
  templateUrl: './watchlist-table.html',
  styleUrls: ['./watchlist-table.css']
})
export class WatchlistTableComponent implements OnInit {
  stocks: Stock[] = [];
  previousPrices: Map<number, number> = new Map();
  loading = false;
  showAddForm = false;
  newStock: CreateStock = {
    ticker: '',
    source: '',
    initialAlertPrice: undefined
  };
  lastUpdated: Date | null = null;
  nextUpdateIn = 60;
  isRefreshing = false;

  displayedColumns: string[] = ['ticker', 'currentPrice', 'nextAlert', 'distance', 'alertCount', 'actions'];

  constructor(
    private watchlistService: WatchlistService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadStocks();
    
    // Auto-refresh DEAKTIVIERT - nur manuell
    // setInterval(() => this.loadStocks(), 60000);
    
    // Countdown timer (nur visuell)
    setInterval(() => {
      if (this.nextUpdateIn > 0 && !this.isRefreshing) {
        this.nextUpdateIn--;
      }
      if (this.nextUpdateIn === 0) {
        this.nextUpdateIn = 60; // Reset, aber kein automatischer Load
      }
    }, 1000);
  }

  loadStocks(manual: boolean = false): void {
    if (manual) this.isRefreshing = true;
    else this.loading = true;
    
    this.watchlistService.getAllStocks().subscribe({
      next: (stocks) => {
        // Store previous prices for animation
        this.stocks.forEach(oldStock => {
          if (oldStock.currentPrice) {
            this.previousPrices.set(oldStock.id, oldStock.currentPrice);
          }
        });
        
        this.stocks = stocks;
        this.lastUpdated = new Date();
        this.nextUpdateIn = 60;
        this.loading = false;
        this.isRefreshing = false;
      },
      error: (error) => {
        console.error('Error loading stocks:', error);
        this.loading = false;
        this.isRefreshing = false;
      }
    });
  }

  addStock(): void {
    if (!this.newStock.ticker) return;

    this.watchlistService.addStock(this.newStock).subscribe({
      next: () => {
        this.loadStocks();
        this.newStock = { ticker: '', source: '', initialAlertPrice: undefined };
        this.showAddForm = false;
      },
      error: (error) => console.error('Error adding stock:', error)
    });
  }

  removeStock(id: number): void {
    if (!confirm('Aktie wirklich entfernen?')) return;

    this.watchlistService.removeStock(id).subscribe({
      next: () => this.loadStocks(),
      error: (error) => console.error('Error removing stock:', error)
    });
  }

  viewDetails(stock: Stock): void {
    this.router.navigate(['/stock', stock.id]);
  }

  getClosestAlert(stock: Stock): { price: number; distance: number } | null {
    if (!stock.currentPrice || stock.alerts.length === 0) return null;

    const alertsWithDistance = stock.alerts.map(alert => ({
      price: alert.targetPrice,
      distance: Math.abs(((alert.targetPrice - stock.currentPrice!) / stock.currentPrice!) * 100)
    }));

    return alertsWithDistance.reduce((min, curr) => 
      curr.distance < min.distance ? curr : min
    );
  }

  getDistanceColor(distance: number): string {
    if (distance < 5) return 'red';
    if (distance < 10) return 'orange';
    return 'gray';
  }

  getPriceChangeClass(stock: Stock): string {
    const previous = this.previousPrices.get(stock.id);
    if (!previous || !stock.currentPrice) return '';
    
    if (stock.currentPrice > previous) return 'price-up';
    if (stock.currentPrice < previous) return 'price-down';
    return '';
  }

  getTimeSinceUpdate(updatedAt?: Date): string {
    if (!updatedAt) return 'Nie aktualisiert';
    
    const now = new Date();
    const updated = new Date(updatedAt);
    const diffMs = now.getTime() - updated.getTime();
    const diffSec = Math.floor(diffMs / 1000);
    
    if (diffSec < 10) return 'gerade eben';
    if (diffSec < 60) return `vor ${diffSec}s`;
    const diffMin = Math.floor(diffSec / 60);
    if (diffMin < 60) return `vor ${diffMin} Min`;
    const diffHour = Math.floor(diffMin / 60);
    return `vor ${diffHour}h`;
  }

  getUpdateStatusColor(updatedAt?: Date): string {
    if (!updatedAt) return 'gray';
    
    const now = new Date();
    const updated = new Date(updatedAt);
    const diffMs = now.getTime() - updated.getTime();
    const diffMin = Math.floor(diffMs / 60000);
    
    if (diffMin < 1) return '#4caf50'; // Green
    if (diffMin < 5) return '#ff9800'; // Orange
    return '#f44336'; // Red
  }
}