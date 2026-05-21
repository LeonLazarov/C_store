using ComputerStore.Application.Common;
using ComputerStore.Application.Dtos;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

public sealed class StockImportService : IStockImportService
{
    private readonly ComputerStoreDbContext _dbContext;

    public StockImportService(ComputerStoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockImportResult> ImportAsync(IReadOnlyCollection<StockImportItem> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            throw new AppException("Import file does not contain any products.");
        }

        var createdProducts = 0;
        var createdCategories = 0;

        foreach (var item in items)
        {
            var name = ServiceHelpers.RequireName(item.Name, "Product name");
            if (item.Price < 0)
            {
                throw new AppException($"Product '{name}' has an invalid price.");
            }

            if (item.Quantity < 0)
            {
                throw new AppException($"Product '{name}' has an invalid quantity.");
            }

            var categoryNames = ServiceHelpers.NormalizeCategories(item.Categories);
            var categories = await ServiceHelpers.GetExistingCategoriesAsync(_dbContext, categoryNames, cancellationToken);

            foreach (var missingName in categoryNames.Except(categories.Select(category => category.Name), StringComparer.OrdinalIgnoreCase))
            {
                var category = new Category { Name = missingName };
                categories.Add(category);
                _dbContext.Categories.Add(category);
                createdCategories++;
            }

            var product = await _dbContext.Products
                .Include(existing => existing.ProductCategories)
                .FirstOrDefaultAsync(existing => existing.Name.ToLower() == name.ToLower(), cancellationToken);

            if (product is null)
            {
                product = new Product { Name = name };
                _dbContext.Products.Add(product);
                createdProducts++;
            }

            product.Price = item.Price;
            product.Quantity = item.Quantity;
            product.ProductCategories.Clear();

            foreach (var category in categories)
            {
                product.ProductCategories.Add(new ProductCategory { Product = product, Category = category });
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new StockImportResult(items.Count, createdProducts, createdCategories);
    }
}
