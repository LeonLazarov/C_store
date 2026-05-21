using ComputerStore.Application.Common;
using ComputerStore.Domain.Entities;
using ComputerStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Infrastructure.Services;

internal static class ServiceHelpers
{
    public static string RequireName(string value, string fieldName)
    {
        var name = value?.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new AppException($"{fieldName} is required.");
        }

        return name;
    }

    public static IReadOnlyCollection<string> NormalizeCategories(IReadOnlyCollection<string>? categories)
    {
        var result = categories?
            .Select(category => category.Trim())
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        if (result.Length == 0)
        {
            throw new AppException("At least one category is required.");
        }

        return result;
    }

    public static async Task<List<Category>> GetExistingCategoriesAsync(
        ComputerStoreDbContext dbContext,
        IReadOnlyCollection<string> categoryNames,
        CancellationToken cancellationToken)
    {
        var loweredNames = categoryNames.Select(category => category.ToLower()).ToArray();

        return await dbContext.Categories
            .Where(category => loweredNames.Contains(category.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }
}
