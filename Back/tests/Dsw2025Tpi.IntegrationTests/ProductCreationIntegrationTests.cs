using System.Net;
using System.Net.Http.Json;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dsw2025Tpi.IntegrationTests;

[Collection("IntegrationTests")]
public class ProductCreationIntegrationTests
{
    private const string ProductsUrl = "/api/products";

    private readonly CustomWebApplicationFactory _factory;

    public ProductCreationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // La base es compartida entre tests: cada test usa un SKU único (máx. 15 caracteres)
    private static string NewSku() => $"SKU-{Guid.NewGuid():N}"[..15];

    private static ProductModel.Request ValidRequest(string sku) =>
        new(sku, $"INT-{sku}", "Mouse Gamer", "Mouse inalámbrico RGB", 15000.50m, 25);

    // CA1 - Política de seguridad
    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]      // anónimo
    [InlineData("Customer", HttpStatusCode.Forbidden)]   // rol ajeno a Admin
    public async Task AddProduct_WhenUnauthenticatedOrNotAdmin_ReturnsUnauthorizedOrForbidden(string? role, HttpStatusCode expectedStatus)
    {
        // Arrange
        var client = role is null ? _factory.CreateClient() : _factory.CreateClientWithRole(role);
        var request = ValidRequest(NewSku());

        // Act
        var response = await client.PostAsJsonAsync(ProductsUrl, request);

        // Assert
        response.StatusCode.Should().Be(expectedStatus);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        (await db.Set<Product>().AnyAsync(p => p.Sku == request.Sku)).Should().BeFalse();
    }

    // CA2 - Persistencia real
    [Fact]
    public async Task AddProduct_WhenAuthenticatedAsAdmin_PersistsInSqliteAndReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClientWithRole("Admin");
        var request = ValidRequest(NewSku());

        // Act
        var response = await client.PostAsJsonAsync(ProductsUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        var saved = await db.Set<Product>().SingleOrDefaultAsync(p => p.Sku == request.Sku);

        saved.Should().NotBeNull();
        saved!.InternalCode.Should().Be(request.InternalCode);
        saved.Name.Should().Be(request.Name);
        saved.Description.Should().Be(request.Description);
        saved.CurrentPrice.Should().Be(request.CurrentPrice);
        saved.StockQuantity.Should().Be(request.StockQuantity);
        saved.IsActive.Should().BeTrue();
    }

    // CA3 - Conflicto HTTP
    [Fact]
    public async Task AddProduct_WhenSkuAlreadyExistsInDatabase_ReturnsConflictStatusCode()
    {
        // Arrange: pre-insertar el producto directamente en SQLite
        var sku = NewSku();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            db.Set<Product>().Add(new Product(sku, $"INT-{sku}", "Teclado Mecánico", "Switches rojos", 30000m, 10));
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClientWithRole("Admin");
        var request = ValidRequest(sku);

        // Act
        var response = await client.PostAsJsonAsync(ProductsUrl, request);

        // Assert: la API maneja la excepción de dominio (409 o 400), nunca 500
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
            (await db.Set<Product>().CountAsync(p => p.Sku == sku)).Should().Be(1);
        }
    }
}
