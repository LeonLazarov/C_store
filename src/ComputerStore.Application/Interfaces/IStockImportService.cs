using ComputerStore.Application.Dtos;

namespace ComputerStore.Application.Interfaces;

public interface IStockImportService
{
    Task<StockImportResult> ImportAsync(IReadOnlyCollection<StockImportItem> items, CancellationToken cancellationToken = default);
}
