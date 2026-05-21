namespace ComputerStore.Application.Dtos;

public sealed record CategoryDto(int Id, string Name, string? Description);

public sealed record CreateCategoryRequest(string Name, string? Description);

public sealed record UpdateCategoryRequest(string Name, string? Description);
