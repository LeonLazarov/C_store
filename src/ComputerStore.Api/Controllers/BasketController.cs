using ComputerStore.Application.Dtos;
using ComputerStore.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ComputerStore.Api.Controllers;

[ApiController]
[Route("api/basket")]
public sealed class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    [HttpPost("discount")]
    public async Task<ActionResult<BasketDiscountResult>> CalculateDiscount(BasketDiscountRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _basketService.CalculateDiscountAsync(request, cancellationToken));
    }
}
