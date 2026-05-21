namespace ComputerStore.Application.Dtos;

public sealed record StockImportItem(
    string Name,
    IReadOnlyCollection<string> Categories,
    decimal Price,
    int Quantity);

public sealed record StockImportResult(int ImportedProducts, int CreatedProducts, int CreatedCategories);
