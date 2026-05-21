using ComputerStore.Application.Dtos;
using ComputerStore.Infrastructure.Persistence;
using ComputerStore.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.UnitTests;

public sealed class StockImportTests
{
    [Fact]
    public async Task Creates_missing_products_and_categories_from_import()
    {
        await using var dbContext = CreateDbContext();
        var service = new StockImportService(dbContext);

        var result = await service.ImportAsync([
            new StockImportItem("Intel's Core i9-9900K", ["CPU"], 475.99m, 2),
            new StockImportItem("Razer BlackWidow Keyboard", ["Keyboard ", "Periphery"], 89.99m, 10)
        ]);

        Assert.Equal(2, result.ImportedProducts);
        Assert.Equal(2, result.CreatedProducts);
        Assert.Equal(3, result.CreatedCategories);
        Assert.Equal(2, await dbContext.Products.CountAsync());
        Assert.Equal(3, await dbContext.Categories.CountAsync());
    }

    private static ComputerStoreDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ComputerStoreDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ComputerStoreDbContext(options);
    }
}
