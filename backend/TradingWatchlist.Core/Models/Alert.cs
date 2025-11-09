namespace TradingWatchlist.Core.Models;

public class Alert
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public decimal TargetPrice { get; set; }
    public bool IsTriggered { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? TriggeredAt { get; set; }
    
    // Navigation property
    public Stock Stock { get; set; } = null!;
}