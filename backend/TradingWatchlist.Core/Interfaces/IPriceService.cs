

// backend/TradingWatchlist.Core/Interfaces/IPriceService.cs
using TradingWatchlist.Core.DTOs;

namespace TradingWatchlist.Core.Interfaces;

public interface IPriceService
{
    Task<PriceQuoteDto?> GetCurrentPriceAsync(string ticker);
    Task<Dictionary<string, decimal>> GetMultiplePricesAsync(List<string> tickers);
}