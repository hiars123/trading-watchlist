using Microsoft.AspNetCore.Mvc;
using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Interfaces;

namespace TradingWatchlist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly IWatchlistService _watchlistService;

    public WatchlistController(IWatchlistService watchlistService)
    {
        _watchlistService = watchlistService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StockDto>>> GetAll()
    {
        var stocks = await _watchlistService.GetAllStocksAsync();
        return Ok(stocks);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockDto>> GetById(int id)
    {
        var stock = await _watchlistService.GetStockByIdAsync(id);
        if (stock == null)
            return NotFound();

        return Ok(stock);
    }

    [HttpPost]
    public async Task<ActionResult<StockDto>> Create(CreateStockDto createDto)
    {
        var stock = await _watchlistService.AddStockAsync(createDto);
        return CreatedAtAction(nameof(GetById), new { id = stock.Id }, stock);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _watchlistService.RemoveStockAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/notes")]
    public async Task<IActionResult> UpdateNotes(int id, [FromBody] string notes)
    {
        var result = await _watchlistService.UpdateStockNotesAsync(id, notes);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id}/source")]
    public async Task<IActionResult> UpdateSource(int id, [FromBody] string source)
    {
        var result = await _watchlistService.UpdateStockSourceAsync(id, source);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
