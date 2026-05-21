namespace ComputerStore.Application.Dtos;

public sealed record ProductDto(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Quantity,
    IReadOnlyCollection<string> Categories);

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Quantity,
    IReadOnlyCollection<string> Categories);

public sealed record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Quantity,
    IReadOnlyCollection<string> Categories);
