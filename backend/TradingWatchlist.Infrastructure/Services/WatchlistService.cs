
// backend/TradingWatchlist.Infrastructure/Services/WatchlistService.cs
using Microsoft.EntityFrameworkCore;
using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Interfaces;
using TradingWatchlist.Core.Models;
using TradingWatchlist.Infrastructure.Data;

namespace TradingWatchlist.Infrastructure.Services;

public class WatchlistService : IWatchlistService
{
    private readonly TradingDbContext _context;
    private readonly IPriceService _priceService;

    public WatchlistService(TradingDbContext context, IPriceService priceService)
    {
        _context = context;
        _priceService = priceService;
    }

    public async Task<List<StockDto>> GetAllStocksAsync()
    {
        var stocks = await _context.Stocks
            .Include(s => s.Alerts)
            .Include(s => s.Screenshots)
            .ToListAsync();

        var tickers = stocks.Select(s => s.Ticker).ToList();
        var pricesWithTimestamp = await GetPricesWithTimestamp(tickers);

        return stocks.Select(s => MapToDto(s, 
            pricesWithTimestamp.GetValueOrDefault(s.Ticker).price,
            pricesWithTimestamp.GetValueOrDefault(s.Ticker).timestamp
        )).ToList();
    }

    public async Task<StockDto?> GetStockByIdAsync(int id)
    {
        var stock = await _context.Stocks
            .Include(s => s.Alerts)
            .Include(s => s.Screenshots)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (stock == null) return null;

        var priceQuote = await _priceService.GetCurrentPriceAsync(stock.Ticker);
        return MapToDto(stock, priceQuote?.Price, priceQuote?.Timestamp);
    }

    private async Task<Dictionary<string, (decimal? price, DateTime? timestamp)>> GetPricesWithTimestamp(List<string> tickers)
    {
        var result = new Dictionary<string, (decimal? price, DateTime? timestamp)>();
        var tasks = tickers.Select(async ticker =>
        {
            var quote = await _priceService.GetCurrentPriceAsync(ticker);
            lock (result)
            {
                result[ticker] = (quote?.Price, quote?.Timestamp);
            }
        });

        await Task.WhenAll(tasks);
        return result;
    }

    public async Task<StockDto> AddStockAsync(CreateStockDto createDto)
    {
        var stock = new Stock
        {
            Ticker = createDto.Ticker.ToUpper(),
            Source = createDto.Source,
            AddedAt = DateTime.UtcNow
        };

        if (createDto.InitialAlertPrice.HasValue)
        {
            stock.Alerts.Add(new Alert
            {
                TargetPrice = createDto.InitialAlertPrice.Value,
                CreatedAt = DateTime.UtcNow
            });
        }

        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return await GetStockByIdAsync(stock.Id) 
            ?? throw new Exception("Failed to retrieve created stock");
    }

    public async Task<bool> RemoveStockAsync(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) return false;

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStockNotesAsync(int id, string notes)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) return false;

        stock.Notes = notes;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateStockSourceAsync(int id, string source)
    {
        var stock = await _context.Stocks.FindAsync(id);
        if (stock == null) return false;

        stock.Source = source;
        await _context.SaveChangesAsync();
        return true;
    }

    private StockDto MapToDto(Stock stock, decimal? currentPrice, DateTime? priceTimestamp = null)
    {
        return new StockDto
        {
            Id = stock.Id,
            Ticker = stock.Ticker,
            AddedAt = stock.AddedAt,
            Source = stock.Source,
            Notes = stock.Notes,
            CurrentPrice = currentPrice,
            PriceUpdatedAt = priceTimestamp,
            ScreenshotCount = stock.Screenshots.Count,
            Alerts = stock.Alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                TargetPrice = a.TargetPrice,
                IsTriggered = a.IsTriggered,
                DistancePercent = (currentPrice.HasValue && currentPrice.Value > 0)
                    ? ((a.TargetPrice - currentPrice.Value) / currentPrice.Value) * 100 
                    : null
            }).ToList()
        };
    }
}
