using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Dsw2025Tpi.IntegrationTests;

[Collection("IntegrationTests")]
public class ProductListingIntegrationTests
{
    private const string ProductsUrl = "/api/products";
    private const string AdminProductsUrl = "/api/products/admin";

    private readonly CustomWebApplicationFactory _factory;

    public ProductListingIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // La base es compartida entre tests: cada test usa un término único en los nombres para aislar sus datos
    private static string NewTerm() => $"Zq{Guid.NewGuid():N}"[..10];

    private async Task<Product> SeedProductAsync(string name, bool isActive = true)
    {
        var sku = $"SKU-{Guid.NewGuid():N}"[..15];
        var product = new Product(sku, $"INT-{sku}", name, "Producto de prueba", 500m, 5);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        db.Set<Product>().Add(product);
        await db.SaveChangesAsync();

        // IsActive tiene HasDefaultValue(true): EF ignora el false al insertar, por eso se desactiva con un UPDATE
        if (!isActive)
        {
            product.IsActive = false;
            await db.SaveChangesAsync();
        }

        return product;
    }

    // CA1 - Contrato JSON público
    [Fact]
    public async Task GetProducts_PublicEndpoint_ReturnsOkWithPaginatedJsonStructure()
    {
        // Arrange
        await SeedProductAsync($"Auriculares {NewTerm()}");
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync(ProductsUrl);

        // Assert: 200 y estructura con productItems y totalItems
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("productItems", out var items).Should().BeTrue();
        json.TryGetProperty("totalItems", out var total).Should().BeTrue();
        items.ValueKind.Should().Be(JsonValueKind.Array);
        total.GetInt32().Should().BeGreaterThan(0);

        var body = json.Deserialize<ProductModel.ResponsePagination>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        body.Should().NotBeNull();
        body!.ProductItems.Should().NotBeEmpty();
    }

    // CA2 - Model Binding de Query Strings
    [Fact]
    public async Task GetProducts_WithQueryStringFilters_ReturnsFilteredResultsFromSqlite()
    {
        // Arrange: 3 productos, solo 2 coinciden con el término de búsqueda
        var term = NewTerm();
        var mouse = await SeedProductAsync($"Mouse {term}");
        var teclado = await SeedProductAsync($"Teclado {term}");
        var monitor = await SeedProductAsync($"Monitor {NewTerm()}");
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"{ProductsUrl}?search={term}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ProductModel.ResponsePagination>();
        body!.TotalItems.Should().Be(2);
        body.ProductItems.Select(p => p.Id).Should().BeEquivalentTo(new[] { mouse.Id, teclado.Id });
        body.ProductItems.Should().NotContain(p => p.Id == monitor.Id);
    }

    // CA3 - Accesibilidad administrativa
    [Fact]
    public async Task GetAuthProducts_AdminEndpoint_WhenCalledByAdmin_IncludesInactiveProducts()
    {
        // Arrange: un producto activo y uno inactivo con el mismo término
        var term = NewTerm();
        var active = await SeedProductAsync($"Parlante {term}");
        var inactive = await SeedProductAsync($"Webcam {term}", isActive: false);
        var adminClient = _factory.CreateClientWithRole("Admin");
        var publicClient = _factory.CreateClient();

        // Act
        var adminResponse = await adminClient.GetAsync($"{AdminProductsUrl}?search={term}&pageNumber=1&pageSize=10");
        var publicResponse = await publicClient.GetAsync($"{ProductsUrl}?search={term}&pageNumber=1&pageSize=10");

        // Assert: el endpoint admin incluye el inactivo
        adminResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var adminBody = await adminResponse.Content.ReadFromJsonAsync<ProductModel.ResponsePagination>();
        adminBody!.ProductItems.Should().Contain(p => p.Id == inactive.Id && p.IsActive == false);
        adminBody.ProductItems.Should().Contain(p => p.Id == active.Id);

        // Assert: a diferencia del catálogo público, que lo excluye
        var publicBody = await publicResponse.Content.ReadFromJsonAsync<ProductModel.ResponsePagination>();
        publicBody!.ProductItems.Should().NotContain(p => p.Id == inactive.Id);
        publicBody.ProductItems.Should().Contain(p => p.Id == active.Id);
    }
}
