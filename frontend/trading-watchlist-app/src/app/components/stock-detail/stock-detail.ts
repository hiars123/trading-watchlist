// frontend/src/app/components/stock-detail/stock-detail.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WatchlistService, Stock, CreateAlert } from '../../services/watchlist.service';

@Component({
  selector: 'app-stock-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './stock-detail.html',
  styleUrls: ['./stock-detail.css']
})
export class StockDetailComponent implements OnInit {
  stock: Stock | null = null;
  loading = false;
  newAlertPrice: number | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private watchlistService: WatchlistService
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.loadStock(id);
  }

  loadStock(id: number): void {
    this.loading = true;
    this.watchlistService.getStockById(id).subscribe({
      next: (stock) => {
        this.stock = stock;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading stock:', error);
        this.loading = false;
      }
    });
  }

  addAlert(): void {
    if (!this.stock || !this.newAlertPrice) return;

    const alert: CreateAlert = {
      stockId: this.stock.id,
      targetPrice: this.newAlertPrice
    };

    this.watchlistService.createAlert(alert).subscribe({
      next: () => {
        this.loadStock(this.stock!.id);
        this.newAlertPrice = null;
      },
      error: (error) => console.error('Error creating alert:', error)
    });
  }

  removeAlert(alertId: number): void {
    if (!confirm('Alarm wirklich löschen?')) return;

    this.watchlistService.deleteAlert(alertId).subscribe({
      next: () => this.loadStock(this.stock!.id),
      error: (error) => console.error('Error deleting alert:', error)
    });
  }

  updateNotes(): void {
    if (!this.stock) return;

    this.watchlistService.updateNotes(this.stock.id, this.stock.notes || '').subscribe({
      error: (error) => console.error('Error updating notes:', error)
    });
  }

  updateSource(): void {
    if (!this.stock) return;

    this.watchlistService.updateSource(this.stock.id, this.stock.source || '').subscribe({
      error: (error) => console.error('Error updating source:', error)
    });
  }

  calculateDistance(alertPrice: number): number | null {
    if (!this.stock?.currentPrice) return null;
    return ((alertPrice - this.stock.currentPrice) / this.stock.currentPrice) * 100;
  }

  getDistanceColor(distance: number): string {
    const abs = Math.abs(distance);
    if (abs < 5) return 'red';
    if (abs < 10) return 'orange';
    return 'gray';
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}