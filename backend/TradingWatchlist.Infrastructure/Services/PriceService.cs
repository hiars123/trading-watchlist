using System.Text.Json;
using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Interfaces;

namespace TradingWatchlist.Infrastructure.Services;

public class PriceService : IPriceService
{
    private readonly HttpClient _httpClient;

    public PriceService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PriceQuoteDto?> GetCurrentPriceAsync(string ticker)
    {
        try
        {
            var url = $"https://query1.finance.yahoo.com/v8/finance/chart/{ticker}?interval=1d&range=1d";
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            
            var price = doc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta")
                .GetProperty("regularMarketPrice")
                .GetDecimal();

            return new PriceQuoteDto
            {
                Ticker = ticker,
                Price = price,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching price for {ticker}: {ex.Message}");
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
}