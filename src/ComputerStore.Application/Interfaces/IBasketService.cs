using ComputerStore.Application.Dtos;

namespace ComputerStore.Application.Interfaces;

public interface IBasketService
{
    Task<BasketDiscountResult> CalculateDiscountAsync(BasketDiscountRequest request, CancellationToken cancellationToken = default);
}
