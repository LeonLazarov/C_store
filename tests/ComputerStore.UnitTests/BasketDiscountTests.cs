using ComputerStore.Application.Dtos;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Persistence;
using ComputerStore.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.UnitTests;

public sealed class BasketDiscountTests
{
    [Fact]
    public async Task Calculates_discount_on_first_copy_when_same_category_quantity_is_more_than_one()
    {
        await using var dbContext = CreateDbContext();
        var cpu = new Category { Name = "CPU" };
        var product = new Product { Name = "Intel's Core i9-9900K", Price = 475.99m, Quantity = 2 };
        product.ProductCategories.Add(new ProductCategory { Product = product, Category = cpu });
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var service = new BasketService(dbContext);
        var result = await service.CalculateDiscountAsync(new BasketDiscountRequest(
            [new BasketItemRequest(product.Id, 2)]));

        Assert.Equal(951.98m, result.Subtotal);
        Assert.Equal(23.80m, result.Discount);
        Assert.Equal(928.18m, result.Total);
    }

    [Fact]
    public async Task Does_not_discount_a_single_product()
    {
        await using var dbContext = CreateDbContext();
        var keyboard = new Category { Name = "Keyboard" };
        var product = new Product { Name = "Razer BlackWidow Keyboard", Price = 89.99m, Quantity = 10 };
        product.ProductCategories.Add(new ProductCategory { Product = product, Category = keyboard });
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync();

        var service = new BasketService(dbContext);
        var result = await service.CalculateDiscountAsync(new BasketDiscountRequest(
            [new BasketItemRequest(product.Id, 1)]));

        Assert.Equal(89.99m, result.Total);
        Assert.Equal(0m, result.Discount);
    }

    [Fact]
    public async Task Rejects_basket_when_not_enough_stock()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Products.Add(new Product { Name = "SSD", Price = 120m, Quantity = 1 });
        await dbContext.SaveChangesAsync();

        var service = new BasketService(dbContext);

        await Assert.ThrowsAsync<ComputerStore.Application.Common.StockAppException>(() =>
            service.CalculateDiscountAsync(new BasketDiscountRequest([new BasketItemRequest(1, 2)])));
    }

    private static ComputerStoreDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ComputerStoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ComputerStoreDbContext(options);
    }
}
