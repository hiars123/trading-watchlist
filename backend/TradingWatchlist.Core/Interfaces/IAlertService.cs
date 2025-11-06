using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Models;

namespace TradingWatchlist.Core.Interfaces;

public interface IAlertService
{
    Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto);
    Task<bool> DeleteAlertAsync(int alertId);
    Task<List<AlertDto>> GetAlertsForStockAsync(int stockId);
    Task CheckAndTriggerAlertsAsync();
}
