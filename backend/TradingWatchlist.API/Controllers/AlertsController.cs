using Microsoft.AspNetCore.Mvc;
using TradingWatchlist.Core.DTOs;
using TradingWatchlist.Core.Interfaces;

namespace TradingWatchlist.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet("stock/{stockId}")]
    public async Task<ActionResult<List<AlertDto>>> GetByStock(int stockId)
    {
        var alerts = await _alertService.GetAlertsForStockAsync(stockId);
        return Ok(alerts);
    }

    [HttpPost]
    public async Task<ActionResult<AlertDto>> Create(CreateAlertDto createDto)
    {
        var alert = await _alertService.CreateAlertAsync(createDto);
        return Ok(alert);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _alertService.DeleteAlertAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }

    [HttpPost("check")]
    public async Task<IActionResult> CheckAlerts()
    {
        await _alertService.CheckAndTriggerAlertsAsync();
        return Ok();
    }
}

