using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Application.Services; 
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions; 
using Dsw2025Tpi.Application.Helpers; // Namespace de tu Extensions

namespace Dsw2025Tpi.UnitTests.Services
{
    public class ProductsManagementServiceTests
{
    private readonly Mock<IRepository> _repositoryMock;
    private readonly Mock<ILogger<ProductsManagementService>> _loggerMock;
    
    // NUEVO: Mock para el logger de las extensiones
    private readonly Mock<ILogger<ProductsManagmentServiceExtensions>> _extensionsLoggerMock; 
    
    private readonly ProductsManagmentServiceExtensions _extensions; 
    private readonly ProductsManagementService _productService;

    public ProductsManagementServiceTests()
    {
        _repositoryMock = new Mock<IRepository>();
        _loggerMock = new Mock<ILogger<ProductsManagementService>>();
        
        // Inicializamos el nuevo logger
        _extensionsLoggerMock = new Mock<ILogger<ProductsManagmentServiceExtensions>>();
        
        // Le pasamos el repositorio Y el logger a las extensiones
        _extensions = new ProductsManagmentServiceExtensions(_repositoryMock.Object, _extensionsLoggerMock.Object);

        // Inyectamos todo al servicio
        _productService = new ProductsManagementService(
            _repositoryMock.Object, 
            _loggerMock.Object, 
            _extensions
        );
    }

        // --- ESCENARIO 1: Camino feliz ---
        [Fact]
        public async Task AddProduct_WithValidData_ReturnsProductAndCallsRepositoryAdd()
        {
            // 1. Arrange: Creamos el request usando tu Record
            var newProductDto = new ProductModel.Request(
                Sku: "SKU-123", 
                InternalCode: "INT-123",
                Name: "Laptop Gamer", 
                Description: "Excelente laptop",
                CurrentPrice: 1500, 
                StockQuantity: 10
            );

            // Simulamos que First<Product> devuelve null (no existe el SKU)
            _repositoryMock.Setup(repo => repo.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Product?)null);

            // 2. Act: Llamamos a tu método real
            var result = await _productService.AddProduct(newProductDto);

            // 3. Assert
            result.Should().NotBeNull();
            result.Sku.Should().Be("SKU-123");

            // Verificamos que se llamó a Add<Product> exactamente una vez
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Once);
        }

        // --- ESCENARIO 2: Validar regla de unicidad de SKU ---
        [Fact]
        public async Task AddProduct_WithDuplicateSku_ThrowsDuplicatedEntityException()
        {
            // Arrange
            var newProductDto = new ProductModel.Request(
                Sku: "SKU-DUPLICADO", 
                InternalCode: "INT-000",
                Name: "Mouse", 
                Description: "Mouse óptico",
                CurrentPrice: 50, 
                StockQuantity: 5
            );
            
            // Simulamos un producto existente
            var existingProduct = new Product("SKU-DUPLICADO", "INT", "Viejo", "Desc", 100, 2);
            
            // Simulamos que el repositorio SÍ ENCUENTRA el SKU
            _repositoryMock.Setup(repo => repo.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync(existingProduct);

            // Act
            Func<Task> act = async () => await _productService.AddProduct(newProductDto);

            // Assert
           await act.Should().ThrowAsync<BadRequestException>();
            
            // Comprobamos que NUNCA se llame a agregar en el repositorio
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
        }

        // --- ESCENARIO 3: Validar reglas de dominio numéricas ---
        [Fact]
        public async Task AddProduct_WithInvalidStockOrPrice_ThrowsInvalidDataException()
        {
            // Arrange
            var invalidProductDto = new ProductModel.Request(
                Sku: "SKU-999", 
                InternalCode: "INT-999",
                Name: "Teclado", 
                Description: "Roto",
                CurrentPrice: -10, // Precio negativo inválido
                StockQuantity: 0   // Stock inválido
            );

            // Act
            Func<Task> act = async () => await _productService.AddProduct(invalidProductDto);

            // Assert
            // (Si tu clase arroja BadRequestException en vez de InvalidDataException, cambialo en la línea de abajo)
            await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
            
            // Comprobamos que el código falle ANTES de llegar a consultar o guardar
            _repositoryMock.Verify(repo => repo.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()), Times.Never);
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
        }
    

// --- ESCENARIO 4: Listar productos excluyendo inactivos ---
        [Fact]
        public async Task GetProducts_WhenIncludeInactiveIsFalse_ReturnsOnlyActiveProducts()
        {
            // Arrange
            var activeProduct = new Product("SKU-ACTIVO", "INT-1", "Activo", "Desc", 100, 10) { IsActive = true };
            var inactiveProduct = new Product("SKU-INACTIVO", "INT-2", "Inactivo", "Desc", 100, 10) { IsActive = false };
            
            var allProducts = new List<Product> { activeProduct, inactiveProduct };

            // Simulamos que el repositorio filtra en base a la expresión enviada por el servicio
            _repositoryMock.Setup(repo => repo.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Expression<Func<Product, bool>> expr, string[] includes) => 
                           {
                               return allProducts.AsQueryable().Where(expr).ToList();
                           });

            // Filtro con Status "enabled" para traer solo los activos
            var filter = new ProductModel.FilterProduct(Status: "enabled", Search: null, PageNumber: 1, PageSize: 10);

            // Act
            var result = await _productService.GetProducts(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalItems.Should().Be(1);
            result.ProductItems.Should().ContainSingle(p => p.Sku == "SKU-ACTIVO");
        }

        // --- ESCENARIO 5: Paginación correcta (Skip y Take) ---
        [Fact]
        public async Task GetProducts_WithPaginationFilter_AppliesSkipAndTakeCorrectly()
        {
            // Arrange: Generamos 15 productos mockeados
            var products = new List<Product>();
            for (int i = 1; i <= 15; i++)
            {
                products.Add(new Product($"SKU-{i}", $"INT-{i}", $"Producto {i}", "Desc", 100, 10) { IsActive = true });
            }

            _repositoryMock.Setup(repo => repo.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Expression<Func<Product, bool>> expr, string[] includes) => 
                           {
                               return products.AsQueryable().Where(expr).ToList();
                           });

            // Solicitamos página 2, tamaño 5
            var filter = new ProductModel.FilterProduct(Status: null, Search: null, PageNumber: 2, PageSize: 5);

            // Act
            var result = await _productService.GetProducts(filter);

            // Assert
            result.Should().NotBeNull();
            result.TotalItems.Should().Be(15);
            result.ProductItems.Should().HaveCount(5); // Exactamente 5 elementos en la segunda página
        }

        // --- ESCENARIO 6: Búsqueda por término (Nombre o SKU) ---
        [Fact]
        public async Task GetProducts_WithSearchTerm_FiltersByNameSkuOrInternalCode()
        {
            // Arrange
            var p1 = new Product("SKU-ABC", "INT-001", "Mouse Gamer", "Desc", 50, 10) { IsActive = true };
            var p2 = new Product("SKU-XYZ", "INT-002", "Teclado Mecánico", "Desc", 80, 5) { IsActive = true };
            var p3 = new Product("SKU-999", "INT-003", "Monitor", "Desc", 200, 2) { IsActive = true };

            var products = new List<Product> { p1, p2, p3 };

            _repositoryMock.Setup(repo => repo.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Expression<Func<Product, bool>> expr, string[] includes) => 
                           {
                               return products.AsQueryable().Where(expr).ToList();
                           });

            // Buscamos por "Mouse"
            var filter = new ProductModel.FilterProduct(Status: null, Search: "Mouse", PageNumber: 1, PageSize: 10);

            // Act
            var result = await _productService.GetProducts(filter);

            // Assert
            result.Should().NotBeNull();
            result.ProductItems.Should().NotBeEmpty();
            result.ProductItems.Should().Contain(p => p.Name == "Mouse Gamer");
        }
}
}