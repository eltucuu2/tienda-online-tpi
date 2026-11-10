using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dsw2025Tpi.IntegrationTests;

[Collection("IntegrationTests")]
public class OrderCreationIntegrationTests
{
    private const string OrdersUrl = "/api/orders";

    private readonly CustomWebApplicationFactory _factory;

    public OrderCreationIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // La base es compartida entre tests: cada test siembra su propio cliente y producto
    private async Task<(Customer Customer, Product Product)> SeedCustomerAndProductAsync(int stock)
    {
        var sku = $"SKU-{Guid.NewGuid():N}"[..15];
        var customer = new Customer($"{Guid.NewGuid():N}@test.com", "Cliente de Prueba", "3815551234");
        var product = new Product(sku, $"INT-{sku}", "Monitor 24", "Monitor Full HD", 1000m, stock);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        db.Set<Customer>().Add(customer);
        db.Set<Product>().Add(product);
        await db.SaveChangesAsync();

        return (customer, product);
    }

    private static OrderModel.Request OrderRequest(Guid customerId, Guid productId, int quantity) =>
        new(customerId, "Av. Siempre Viva 742", "Av. Siempre Viva 742",
            new List<OrderItemModel.Request> { new(productId, quantity) });

    // CA1 - Endpoint protegido
    [Fact]
    public async Task CreateOrder_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = OrderRequest(Guid.NewGuid(), Guid.NewGuid(), 1);

        // Act
        var response = await client.PostAsJsonAsync(OrdersUrl, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // CA2 - Transacción completa SQLite
    [Fact]
    public async Task CreateOrder_WhenAuthenticatedWithValidData_ReturnsCreatedAndPersistsOrderAndStockInSqlite()
    {
        // Arrange: cliente y producto con stock inicial 10
        var (customer, product) = await SeedCustomerAndProductAsync(stock: 10);
        var client = _factory.CreateClientWithRole("Customer");
        var request = OrderRequest(customer.Id, product.Id, quantity: 3);

        // Act
        var response = await client.PostAsJsonAsync(OrdersUrl, request);

        // Assert: respuesta HTTP
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var orderId = body.GetProperty("id").GetGuid();

        // Assert: estado real en SQLite
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

        var order = await db.Set<Order>().Include(o => o.Items).SingleOrDefaultAsync(o => o.Id == orderId);
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(customer.Id);
        order.Items.Should().ContainSingle(i => i.ProductId == product.Id && i.Quantity == 3);

        var savedProduct = await db.Set<Product>().SingleAsync(p => p.Id == product.Id);
        savedProduct.StockQuantity.Should().Be(7);
    }

    // CA3 - Manejo global de error
    [Fact]
    public async Task CreateOrder_WhenStockIsInsufficient_ReturnsBadRequestWithErrorMessage()
    {
        // Arrange: producto con stock 2 y una orden por 5 unidades
        var (customer, product) = await SeedCustomerAndProductAsync(stock: 2);
        var client = _factory.CreateClientWithRole("Customer");
        var request = OrderRequest(customer.Id, product.Id, quantity: 5);

        // Act
        var response = await client.PostAsJsonAsync(OrdersUrl, request);

        // Assert: 400 con un JSON que explica la falta de stock
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("message").GetString().Should().ContainEquivalentOf("stock");

        // Assert: no se creó la orden ni se tocó el stock
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

        (await db.Set<Order>().AnyAsync(o => o.CustomerId == customer.Id)).Should().BeFalse();
        (await db.Set<Product>().SingleAsync(p => p.Id == product.Id)).StockQuantity.Should().Be(2);
    }
}
