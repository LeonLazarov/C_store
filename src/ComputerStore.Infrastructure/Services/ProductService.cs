using AutoMapper;
using ComputerStore.Application.Common;
using ComputerStore.Application.Dtos;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

public sealed class ProductService : IProductService
{
    private readonly ComputerStoreDbContext _dbContext;
    private readonly IMapper _mapper;

    public ProductService(ComputerStoreDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var products = await BaseQuery().OrderBy(product => product.Name).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<ProductDto>>(products);
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _mapper.Map<ProductDto>(await FindProductAsync(id, cancellationToken));
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = new Product();
        await ApplyChangesAsync(product, request.Name, request.Description, request.Price, request.Quantity, request.Categories, cancellationToken);

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(await FindProductAsync(product.Id, cancellationToken));
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await FindProductAsync(id, cancellationToken);
        await ApplyChangesAsync(product, request.Name, request.Description, request.Price, request.Quantity, request.Categories, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductDto>(product);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await FindProductAsync(id, cancellationToken);
        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Product> BaseQuery()
    {
        return _dbContext.Products
            .Include(product => product.ProductCategories)
            .ThenInclude(productCategory => productCategory.Category);
    }

    private async Task<Product> FindProductAsync(int id, CancellationToken cancellationToken)
    {
        return await BaseQuery().FirstOrDefaultAsync(product => product.Id == id, cancellationToken)
            ?? throw new NotFoundAppException($"Product with id {id} was not found.");
    }

    private async Task ApplyChangesAsync(
        Product product,
        string nameValue,
        string? description,
        decimal price,
        int quantity,
        IReadOnlyCollection<string> categoryValues,
        CancellationToken cancellationToken)
    {
        var name = ServiceHelpers.RequireName(nameValue, "Product name");
        if (price < 0)
        {
            throw new AppException("Price cannot be negative.");
        }

        if (quantity < 0)
        {
            throw new AppException("Quantity cannot be negative.");
        }

        var categoryNames = ServiceHelpers.NormalizeCategories(categoryValues);
        var categories = await ServiceHelpers.GetExistingCategoriesAsync(_dbContext, categoryNames, cancellationToken);
        var missingCategories = categoryNames.Except(categories.Select(category => category.Name), StringComparer.OrdinalIgnoreCase).ToArray();

        if (missingCategories.Length > 0)
        {
            throw new AppException($"These categories do not exist: {string.Join(", ", missingCategories)}.");
        }

        var duplicateName = await _dbContext.Products.AnyAsync(existing =>
            existing.Name.ToLower() == name.ToLower() && existing.Id != product.Id, cancellationToken);

        if (duplicateName)
        {
            throw new AppException($"Product '{name}' already exists.");
        }

        product.Name = name;
        product.Description = description?.Trim();
        product.Price = price;
        product.Quantity = quantity;
        product.ProductCategories.Clear();

        foreach (var category in categories)
        {
            product.ProductCategories.Add(new ProductCategory { Product = product, Category = category });
        }
    }
}
