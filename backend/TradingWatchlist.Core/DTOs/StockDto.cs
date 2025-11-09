// backend/TradingWatchlist.Core/DTOs/StockDto.cs
namespace TradingWatchlist.Core.DTOs;

public class StockDto
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public decimal? CurrentPrice { get; set; }
    public DateTime? PriceUpdatedAt { get; set; }
    public List<AlertDto> Alerts { get; set; } = new();
    public int ScreenshotCount { get; set; }
}

public class CreateStockDto
{
    public string Ticker { get; set; } = string.Empty;
    public string? Source { get; set; }
    public decimal? InitialAlertPrice { get; set; }
}

public class AlertDto
{
    public int Id { get; set; }
    public decimal TargetPrice { get; set; }
    public bool IsTriggered { get; set; }
    public decimal? DistancePercent { get; set; }
}

public class CreateAlertDto
{
    public int StockId { get; set; }
    public decimal TargetPrice { get; set; }
}

public class PriceQuoteDto
{
    public string Ticker { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}