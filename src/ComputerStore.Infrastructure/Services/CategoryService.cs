using AutoMapper;
using ComputerStore.Application.Common;
using ComputerStore.Application.Dtos;
using ComputerStore.Application.Interfaces;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ComputerStoreDbContext _dbContext;
    private readonly IMapper _mapper;

    public CategoryService(ComputerStoreDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories.OrderBy(category => category.Name).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyCollection<CategoryDto>>(categories);
    }

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await FindCategoryAsync(id, cancellationToken);
        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = ServiceHelpers.RequireName(request.Name, "Category name");
        await EnsureUniqueNameAsync(name, null, cancellationToken);

        var category = new Category { Name = name, Description = request.Description?.Trim() };
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var category = await FindCategoryAsync(id, cancellationToken);
        var name = ServiceHelpers.RequireName(request.Name, "Category name");
        await EnsureUniqueNameAsync(name, id, cancellationToken);

        category.Name = name;
        category.Description = request.Description?.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await FindCategoryAsync(id, cancellationToken);
        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> FindCategoryAsync(int id, CancellationToken cancellationToken)
    {
        return await _dbContext.Categories.FindAsync([id], cancellationToken)
            ?? throw new NotFoundAppException($"Category with id {id} was not found.");
    }

    private async Task EnsureUniqueNameAsync(string name, int? currentId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Categories.AnyAsync(category =>
            category.Name.ToLower() == name.ToLower() && (!currentId.HasValue || category.Id != currentId.Value),
            cancellationToken);

        if (exists)
        {
            throw new AppException($"Category '{name}' already exists.");
        }
    }
}
