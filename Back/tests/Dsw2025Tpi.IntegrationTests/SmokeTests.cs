using System.Net;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Dsw2025Tpi.IntegrationTests;

[Collection("IntegrationTests")]
public class SmokeTests
{
    private readonly CustomWebApplicationFactory _factory;

    public SmokeTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Api_ArrancaConSqlite_HealthcheckDevuelveOk()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/healthcheck");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Confirma que el esquema se creó en SQLite (la consulta falla si la tabla no existe)
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();
        db.Set<Product>().Count().Should().Be(0);
    }
}