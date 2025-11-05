import { ComponentFixture, TestBed } from '@angular/core/testing';

import { WatchlistTable } from './watchlist-table';

describe('WatchlistTable', () => {
  let component: WatchlistTable;
  let fixture: ComponentFixture<WatchlistTable>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [WatchlistTable]
    })
    .compileComponents();

    fixture = TestBed.createComponent(WatchlistTable);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
