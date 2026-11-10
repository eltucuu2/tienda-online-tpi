namespace Dsw2025Tpi.IntegrationTests;

// Todas las clases con [Collection("IntegrationTests")] comparten una sola factory y corren en serie
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestsCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}