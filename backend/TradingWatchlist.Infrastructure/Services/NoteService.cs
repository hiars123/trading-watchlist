using TradingWatchlist.Core.Interfaces;
using TradingWatchlist.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TradingWatchlist.Infrastructure.Services;

public class NoteService : INoteService
{
    private readonly TradingDbContext _context;

    public NoteService(TradingDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UpdateNotesAsync(int stockId, string notes)
    {
        var stock = await _context.Stocks.FindAsync(stockId);
        if (stock == null) return false;

        stock.Notes = notes;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string?> GetNotesAsync(int stockId)
    {
        var stock = await _context.Stocks.FindAsync(stockId);
        return stock?.Notes;
    }
}