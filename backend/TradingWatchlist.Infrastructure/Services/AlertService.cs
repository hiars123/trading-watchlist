using Microsoft.EntityFrameworkCore;
using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Interfaces;
using TradingWatchlist.Core.Models;
using TradingWatchlist.Infrastructure.Data;

namespace TradingWatchlist.Infrastructure.Services;

public class AlertService : IAlertService
{
    private readonly TradingDbContext _context;
    private readonly IPriceService _priceService;

    public AlertService(TradingDbContext context, IPriceService priceService)
    {
        _context = context;
        _priceService = priceService;
    }

    public async Task<AlertDto> CreateAlertAsync(CreateAlertDto createDto)
    {
        var alert = new Alert
        {
            StockId = createDto.StockId,
            TargetPrice = createDto.TargetPrice,
            CreatedAt = DateTime.UtcNow
        };

        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();

        return new AlertDto
        {
            Id = alert.Id,
            TargetPrice = alert.TargetPrice,
            IsTriggered = alert.IsTriggered
        };
    }

    public async Task<bool> DeleteAlertAsync(int alertId)
    {
        var alert = await _context.Alerts.FindAsync(alertId);
        if (alert == null) return false;

        _context.Alerts.Remove(alert);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<AlertDto>> GetAlertsForStockAsync(int stockId)
    {
        var alerts = await _context.Alerts
            .Where(a => a.StockId == stockId)
            .ToListAsync();

        return alerts.Select(a => new AlertDto
        {
            Id = a.Id,
            TargetPrice = a.TargetPrice,
            IsTriggered = a.IsTriggered
        }).ToList();
    }

    public async Task CheckAndTriggerAlertsAsync()
    {
        var alerts = await _context.Alerts
            .Include(a => a.Stock)
            .Where(a => !a.IsTriggered)
            .ToListAsync();

        foreach (var alert in alerts)
        {
            var quote = await _priceService.GetCurrentPriceAsync(alert.Stock.Ticker);
            if (quote == null) continue;

            // Simple trigger logic: alert if price crosses target
            if (Math.Abs(quote.Price - alert.TargetPrice) / alert.TargetPrice < 0.01m) // Within 1%
            {
                alert.IsTriggered = true;
                alert.TriggeredAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
    }
}
