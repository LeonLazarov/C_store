using System.Net;
using System.Net.Http.Json;
using ComputerStore.Application.Dtos;

namespace ComputerStore.IntegrationTests;

public sealed class ApiTests : IClassFixture<ComputerStoreApiFactory>
{
    private readonly HttpClient _client;

    public ApiTests(ComputerStoreApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Category_crud_endpoint_creates_category()
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest("Storage", "Drives and media"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var category = await response.Content.ReadFromJsonAsync<CategoryDto>();
        Assert.NotNull(category);
        Assert.Equal("Storage", category.Name);
    }

    [Fact]
    public async Task Stock_import_and_discount_endpoints_work_together()
    {
        var importResponse = await _client.PostAsJsonAsync("/api/stock/import", new[]
        {
            new StockImportItem("Intel's Core i9-9900K", ["CPU"], 475.99m, 2),
            new StockImportItem("Razer BlackWidow Keyboard", ["Keyboard", "Periphery"], 89.99m, 10)
        });
        importResponse.EnsureSuccessStatusCode();

        var products = await _client.GetFromJsonAsync<ProductDto[]>("/api/products");
        Assert.NotNull(products);
        var cpu = products.Single(product => product.Name == "Intel's Core i9-9900K");
        var keyboard = products.Single(product => product.Name == "Razer BlackWidow Keyboard");

        var discountResponse = await _client.PostAsJsonAsync("/api/basket/discount",
            new BasketDiscountRequest([
                new BasketItemRequest(cpu.Id, 2),
                new BasketItemRequest(keyboard.Id, 1)
            ]));

        discountResponse.EnsureSuccessStatusCode();
        var discount = await discountResponse.Content.ReadFromJsonAsync<BasketDiscountResult>();
        Assert.NotNull(discount);
        Assert.Equal(23.80m, discount.Discount);
    }
}
