using ComputerStore.Application.Dtos;
using ComputerStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Api.Controllers;

[ApiController]
[Route("api/stock")]
public sealed class StockController : ControllerBase
{
    private readonly IStockImportService _stockImportService;

    public StockController(IStockImportService stockImportService)
    {
        _stockImportService = stockImportService;
    }

    [HttpPost("import")]
    public async Task<ActionResult<StockImportResult>> Import(IReadOnlyCollection<StockImportItem> items, CancellationToken cancellationToken)
    {
        return Ok(await _stockImportService.ImportAsync(items, cancellationToken));
    }
}
