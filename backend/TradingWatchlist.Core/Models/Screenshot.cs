namespace TradingWatchlist.Core.Models;

public class Screenshot
{
    public int Id { get; set; }
    public int StockId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Stock Stock { get; set; } = null!;
}