using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Linq;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Application.Services; 
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions; 
using Dsw2025Tpi.Application.Helpers;

namespace Dsw2025Tpi.UnitTests.Services
{
    public class ProductsManagementServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly Mock<ILogger<ProductsManagementService>> _loggerMock;
        private readonly Mock<ILogger<ProductsManagmentServiceExtensions>> _extensionsLoggerMock; 
        private readonly ProductsManagmentServiceExtensions _extensions; 
        private readonly ProductsManagementService _productService;

        public ProductsManagementServiceTests()
        {
            _repositoryMock = new Mock<IRepository>();
            _loggerMock = new Mock<ILogger<ProductsManagementService>>();
            _extensionsLoggerMock = new Mock<ILogger<ProductsManagmentServiceExtensions>>();
            
            _extensions = new ProductsManagmentServiceExtensions(_repositoryMock.Object, _extensionsLoggerMock.Object);

            _productService = new ProductsManagementService(
                _repositoryMock.Object, 
                _loggerMock.Object, 
                _extensions
            );
        }

        [Fact]
        public async Task AddProduct_WithValidData_ReturnsProductAndCallsRepositoryAdd()
        {
            var newProductDto = new ProductModel.Request(
                Sku: "SKU-123", 
                InternalCode: "INT-123",
                Name: "Laptop Gamer", 
                Description: "Excelente laptop",
                CurrentPrice: 1500, 
                StockQuantity: 10
            );

            _repositoryMock.Setup(repo => repo.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Product?)null);

            var result = await _productService.AddProduct(newProductDto);

            result.Should().NotBeNull();
            result!.Sku.Should().Be("SKU-123");

            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async Task AddProduct_WithDuplicateSku_ThrowsDuplicatedEntityException()
        {
            var newProductDto = new ProductModel.Request(
                Sku: "SKU-DUPLICADO", 
                InternalCode: "INT-000",
                Name: "Mouse", 
                Description: "Mouse óptico",
                CurrentPrice: 50, 
                StockQuantity: 5
            );
            
            var existingProduct = new Product("SKU-DUPLICADO", "INT", "Viejo", "Desc", 100, 2);
            
            _repositoryMock.Setup(repo => repo.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync(existingProduct);

            Func<Task> act = async () => await _productService.AddProduct(newProductDto);

            await act.Should().ThrowAsync<DuplicatedEntityException>();
            
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task AddProduct_WithInvalidStockOrPrice_ThrowsBadRequestException()
        {
            var invalidProductDto = new ProductModel.Request(
                Sku: "SKU-999", 
                InternalCode: "INT-999",
                Name: "Teclado", 
                Description: "Roto",
                CurrentPrice: -10,
                StockQuantity: 0
            );

            Func<Task> act = async () => await _productService.AddProduct(invalidProductDto);

            await act.Should().ThrowAsync<BadRequestException>();
            
            _repositoryMock.Verify(repo => repo.First(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()), Times.Never);
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task GetProducts_WhenIncludeInactiveIsFalse_ReturnsOnlyActiveProducts()
        {
            var activeProduct = new Product("SKU-ACTIVO", "INT-1", "Activo", "Desc", 100, 10) { IsActive = true };
            var inactiveProduct = new Product("SKU-INACTIVO", "INT-2", "Inactivo", "Desc", 100, 10) { IsActive = false };
            
            var allProducts = new List<Product> { activeProduct, inactiveProduct };

            _repositoryMock.Setup(repo => repo.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Expression<Func<Product, bool>> expr, string[] includes) => 
                           {
                               return allProducts.AsQueryable().Where(expr).ToList();
                           });

            var filter = new ProductModel.FilterProduct(Status: "enabled", Search: null, PageNumber: 1, PageSize: 10);

            var result = await _productService.GetProducts(filter);

            result.Should().NotBeNull();
            result!.TotalItems.Should().Be(1);
            result.ProductItems.Should().ContainSingle(p => p.Sku == "SKU-ACTIVO");
        }

        [Fact]
        public async Task GetProducts_WithPaginationFilter_AppliesSkipAndTakeCorrectly()
        {
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

            var filter = new ProductModel.FilterProduct(Status: null, Search: null, PageNumber: 2, PageSize: 5);

            var result = await _productService.GetProducts(filter);

            result.Should().NotBeNull();
            result!.TotalItems.Should().Be(15);
            result.ProductItems.Should().HaveCount(5);
            result.ProductItems!.First().Sku.Should().Be("SKU-6");
            result.ProductItems!.Last().Sku.Should().Be("SKU-10");
        }

        [Fact]
       [Fact]
        public async Task GetProducts_WithSearchTerm_FiltersByNameSkuOrInternalCode()
        {
            var p1 = new Product("SKU-ABC", "INT-001", "Mouse Gamer", "Desc", 50, 10) { IsActive = true };
            var p2 = new Product("SKU-XYZ", "INT-002", "Teclado Mecánico", "Desc", 80, 5) { IsActive = true };
            var p3 = new Product("SKU-999", "INT-003", "Monitor", "Desc", 200, 2) { IsActive = true };

            var products = new List<Product> { p1, p2, p3 };

            _repositoryMock.Setup(repo => repo.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Expression<Func<Product, bool>> expr, string[] includes) => 
                           {
                               return products.AsQueryable().Where(expr.Compile()).ToList();
                           });

            var filter = new ProductModel.FilterProduct(Status: null, Search: "mouse", PageNumber: 1, PageSize: 10);

            var result = await _productService.GetProducts(filter);

            result.Should().NotBeNull();
            result!.ProductItems.Should().HaveCount(1);
            result.ProductItems!.First().Name.Should().Be("Mouse Gamer");
        }
    }
}