// frontend/src/app/app.routes.ts
import { Routes } from '@angular/router';
import { WatchlistTableComponent } from './components/watchlist-table/watchlist-table';
import { StockDetailComponent } from './components/stock-detail/stock-detail';

export const routes: Routes = [
  { path: '', component: WatchlistTableComponent },
  { path: 'stock/:id', component: StockDetailComponent },
  { path: '**', redirectTo: '' }
];