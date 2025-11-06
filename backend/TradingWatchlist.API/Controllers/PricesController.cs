using Microsoft.AspNetCore.Mvc;
using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Interfaces;

namespace TradingWatchlist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PricesController : ControllerBase
{
    private readonly IPriceService _priceService;

    public PricesController(IPriceService priceService)
    {
        _priceService = priceService;
    }

    [HttpGet("{ticker}")]
    public async Task<ActionResult<PriceQuoteDto>> GetPrice(string ticker)
    {
        var quote = await _priceService.GetCurrentPriceAsync(ticker);
        if (quote == null)
            return NotFound();

        return Ok(quote);
    }

    [HttpPost("batch")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetMultiplePrices([FromBody] List<string> tickers)
    {
        var prices = await _priceService.GetMultiplePricesAsync(tickers);
        return Ok(prices);
    }
}