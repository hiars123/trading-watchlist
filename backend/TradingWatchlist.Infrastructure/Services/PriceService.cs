
// backend/TradingWatchlist.Infrastructure/Services/PriceService.cs
using System.Text.Json;
using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Interfaces;

namespace TradingWatchlist.Infrastructure.Services;

public class PriceService : IPriceService
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, CachedPrice> _priceCache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
    private readonly object _cacheLock = new();

    public PriceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PriceQuoteDto?> GetCurrentPriceAsync(string ticker)
    {
        // Check cache first
        lock (_cacheLock)
        {
            if (_priceCache.TryGetValue(ticker, out var cached))
            {
                if (DateTime.UtcNow - cached.FetchedAt < _cacheExpiry)
                {
                    Console.WriteLine($"[CACHE HIT] {ticker}: ${cached.Price} (cached at {cached.FetchedAt:HH:mm:ss})");
                    return new PriceQuoteDto
                    {
                        Ticker = ticker,
                        Price = cached.Price,
                        Timestamp = cached.FetchedAt
                    };
                }
                else
                {
                    Console.WriteLine($"[CACHE EXPIRED] {ticker} - fetching new data");
                    _priceCache.Remove(ticker);
                }
            }
        }

        // Fetch from Yahoo Finance
        try
        {
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?interval=1d&range=1d";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[YAHOO API] {ticker} - HTTP {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            
            var price = doc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta")
                .GetProperty("regularMarketPrice")
                .GetDecimal();

            var now = DateTime.UtcNow;
            var quote = new PriceQuoteDto
            {
                Ticker = ticker,
                Price = price,
                Timestamp = now
            };

            // Cache the result
            lock (_cacheLock)
            {
                _priceCache[ticker] = new CachedPrice
                {
                    Price = price,
                    FetchedAt = now
                };
            }

            Console.WriteLine($"[YAHOO API] {ticker}: ${price} (fetched at {now:HH:mm:ss})");
            return quote;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ticker}: {ex.Message}");
            return null;
        }
    }

    public async Task<Dictionary<string, decimal>> GetMultiplePricesAsync(List<string> tickers)
    {
        var prices = new Dictionary<string, decimal>();
        var tasks = tickers.Select(async ticker =>
        {
            var quote = await GetCurrentPriceAsync(ticker);
            if (quote != null)
            {
                lock (prices)
                {
                    prices[ticker] = quote.Price;
                }
            }
        });

        await Task.WhenAll(tasks);
        return prices;
    }

    // Helper class for caching
    private class CachedPrice
    {
        public decimal Price { get; set; }
        public DateTime FetchedAt { get; set; }
    }
}
