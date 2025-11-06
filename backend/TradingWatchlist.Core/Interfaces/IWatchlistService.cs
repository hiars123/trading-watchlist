using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Models;

namespace TradingWatchlist.Core.Interfaces;

public interface IWatchlistService
{
    Task<List<StockDto>> GetAllStocksAsync();
    Task<StockDto?> GetStockByIdAsync(int id);
    Task<StockDto> AddStockAsync(CreateStockDto createDto);
    Task<bool> RemoveStockAsync(int id);
    Task<bool> UpdateStockNotesAsync(int id, string notes);
    Task<bool> UpdateStockSourceAsync(int id, string source);
}