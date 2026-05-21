using ComputerStore.Application.Common;
using ComputerStore.Application.Dtos;
using ComputerStore.Application.Interfaces;
using ComputerStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

public sealed class BasketService : IBasketService
{
    private const decimal DiscountRate = 0.05m;
    private readonly ComputerStoreDbContext _dbContext;

    public BasketService(ComputerStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BasketDiscountResult> CalculateDiscountAsync(BasketDiscountRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            throw new AppException("Basket must contain at least one product.");
        }

        var normalizedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new BasketItemRequest(group.Key, group.Sum(item => item.Quantity)))
            .ToArray();

        if (normalizedItems.Any(item => item.Quantity <= 0))
        {
            throw new AppException("Basket quantities must be greater than zero.");
        }

        var productIds = normalizedItems.Select(item => item.ProductId).ToArray();
        var products = await _dbContext.Products
            .Include(product => product.ProductCategories)
            .ThenInclude(productCategory => productCategory.Category)
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);

        var missingIds = productIds.Except(products.Select(product => product.Id)).ToArray();
        if (missingIds.Length > 0)
        {
            throw new NotFoundAppException($"Products were not found: {string.Join(", ", missingIds)}.");
        }

        foreach (var item in normalizedItems)
        {
            var product = products.Single(product => product.Id == item.ProductId);
            if (item.Quantity > product.Quantity)
            {
                throw new StockAppException($"There are only {product.Quantity} units of '{product.Name}' in stock.");
            }
        }

        var categoryQuantities = products
            .SelectMany(product =>
            {
                var quantity = normalizedItems.Single(item => item.ProductId == product.Id).Quantity;
                return product.ProductCategories.Select(productCategory => new
                {
                    productCategory.CategoryId,
                    Quantity = quantity
                });
            })
            .GroupBy(item => item.CategoryId)
            .ToDictionary(group => group.Key, group => group.Sum(item => item.Quantity));

        var lines = products.Select(product =>
        {
            var quantity = normalizedItems.Single(item => item.ProductId == product.Id).Quantity;
            var qualifies = quantity > 1 || product.ProductCategories.Any(productCategory =>
                categoryQuantities.TryGetValue(productCategory.CategoryId, out var categoryQuantity) && categoryQuantity > 1);
            var discount = qualifies ? Math.Round(product.Price * DiscountRate, 2, MidpointRounding.AwayFromZero) : 0m;

            return new BasketLineResult(
                product.Id,
                product.Name,
                quantity,
                product.Price,
                product.Price * quantity,
                discount);
        }).ToArray();

        var subtotal = lines.Sum(line => line.LineTotal);
        var discountTotal = lines.Sum(line => line.Discount);

        return new BasketDiscountResult(subtotal, discountTotal, subtotal - discountTotal, lines);
    }
}
