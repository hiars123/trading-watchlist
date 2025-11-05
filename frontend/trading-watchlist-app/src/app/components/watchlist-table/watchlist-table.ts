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
  loading = false;
  showAddForm = false;
  newStock: CreateStock = {
    ticker: '',
    source: '',
    initialAlertPrice: undefined
  };

  displayedColumns: string[] = ['ticker', 'currentPrice', 'nextAlert', 'distance', 'alertCount', 'actions'];

  constructor(
    private watchlistService: WatchlistService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadStocks();
    // Auto-refresh every minute
    setInterval(() => this.loadStocks(), 60000);
  }

  loadStocks(): void {
    this.loading = true;
    this.watchlistService.getAllStocks().subscribe({
      next: (stocks) => {
        this.stocks = stocks;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading stocks:', error);
        this.loading = false;
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
}