namespace TradingWatchlist.Core.Interfaces;

public interface INoteService
{
    Task<bool> UpdateNotesAsync(int stockId, string notes);
    Task<string?> GetNotesAsync(int stockId);
}