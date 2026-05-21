namespace ComputerStore.Application.Dtos;

public sealed record BasketItemRequest(int ProductId, int Quantity);

public sealed record BasketDiscountRequest(IReadOnlyCollection<BasketItemRequest> Items);

public sealed record BasketLineResult(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal Discount);

public sealed record BasketDiscountResult(
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    IReadOnlyCollection<BasketLineResult> Lines);
