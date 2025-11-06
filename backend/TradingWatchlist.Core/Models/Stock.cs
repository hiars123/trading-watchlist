namespace TradingWatchlist.Core.Models;

public class Stock
{
    public int Id { get; set; }
    public string Ticker { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public string? Source { get; set; }
    public string? Notes { get; set; }
    
    // Navigation properties
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
    public ICollection<Screenshot> Screenshots { get; set; } = new List<Screenshot>();
}
