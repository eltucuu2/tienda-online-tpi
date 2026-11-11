using Moq;
using Xunit;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Collections.Generic;
using Dsw2025Tpi.Domain.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Application.Services; 
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Helpers;

namespace Dsw2025Tpi.UnitTests.Services
{
    public class OrdersManagementServiceTests
    {
        private readonly Mock<IRepository> _repositoryMock;
        private readonly Mock<ILogger<OrdersManagementService>> _loggerMock;
        private readonly Mock<ILogger<OrdersManagementServiceExtensions>> _extensionsLoggerMock;
        
        private readonly OrdersManagementServiceExtensions _extensions; 
        private readonly OrdersManagementService _orderService;

        public OrdersManagementServiceTests()
        {
            _repositoryMock = new Mock<IRepository>();
            _loggerMock = new Mock<ILogger<OrdersManagementService>>();
            _extensionsLoggerMock = new Mock<ILogger<OrdersManagementServiceExtensions>>();
            
            _extensions = new OrdersManagementServiceExtensions(_repositoryMock.Object, _extensionsLoggerMock.Object);

            _orderService = new OrdersManagementService(
                _repositoryMock.Object, 
                _loggerMock.Object, 
                _extensions
            );
        }

        // --- ESCENARIO 1: Camino feliz ---
        [Fact]
        public async Task CreateOrder_WithValidItemsAndStock_CalculatesTotalAndDecrementsProductStock()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            var createOrderRequest = new OrderModel.Request(
                CustomerId: customerId,
                ShippingAddress: "Calle Falsa 123",
                BillingAddress: "Calle Falsa 123",
                OrderItems: new List<OrderItemModel.Request> 
                {
                    // Pedimos 2 unidades
                    new OrderItemModel.Request(ProductId: productId, Quantity: 2)
                }
            );

            var existingCustomer = new Customer { Id = customerId }; 
            
            // Producto original: 10 en stock, precio 50
            var existingProduct = new Product("SKU-1", "INT-1", "Prod", "Desc", 50, 10) 
            { 
                Id = productId,
                IsActive = true 
            };

            // Simulamos que el cliente existe
            _repositoryMock.Setup(repo => repo.GetById<Customer>(It.IsAny<Guid>(), It.IsAny<string[]>()))
                           .ReturnsAsync(existingCustomer);
                           
            // Simulamos que el producto existe en la base de datos
            _repositoryMock.Setup(repo => repo.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync(new List<Product> { existingProduct });

            // Act
            var result = await _orderService.AddOrder(createOrderRequest); 

            // Assert
            result.Should().NotBeNull();
            
            // Verifica que el cálculo contable (2 * 50) se hizo correctamente (si tu entidad Order calcula el total)
            result.TotalAmount.Should().Be(100);

            // Verifica que se descontó el stock del objeto en memoria (10 original - 2 comprados = 8)
            existingProduct.StockQuantity.Should().Be(8);

            // Verifica que se actualizó el stock y se guardó la orden en BD
            _repositoryMock.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Once);
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Once);
        }

        // --- ESCENARIO 2: Validar la protección contra sobreventa (out-of-stock) ---
        [Fact]
        public async Task CreateOrder_WhenProductStockIsInsufficient_ThrowsInsufficientStockException()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            
            var createOrderRequest = new OrderModel.Request(
                CustomerId: customerId,
                ShippingAddress: "Calle Falsa 123",
                BillingAddress: "Calle Falsa 123",
                OrderItems: new List<OrderItemModel.Request> 
                {
                    new OrderItemModel.Request(ProductId: productId, Quantity: 5) // Pedimos 5
                }
            );

            var existingCustomer = new Customer { Id = customerId };
            
            // Producto con stock INSUFICIENTE (Solo tiene 2)
            var existingProduct = new Product("SKU-2", "INT-2", "Prod2", "Desc", 50, 2) 
            { 
                Id = productId,
                IsActive = true
            };

            _repositoryMock.Setup(repo => repo.GetById<Customer>(It.IsAny<Guid>(), It.IsAny<string[]>()))
                           .ReturnsAsync(existingCustomer);
                           
            _repositoryMock.Setup(repo => repo.GetFiltered(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string[]>()))
                           .ReturnsAsync(new List<Product> { existingProduct });

            // Act
            Func<Task> act = async () => await _orderService.AddOrder(createOrderRequest);

            // Assert - Tu código lanza BadRequestException por la falta de stock
            await act.Should().ThrowAsync<BadRequestException>()
                     .WithMessage("*no tiene stock suficiente*"); 
            
            // Comprobamos que corta antes de guardar nada
            _repositoryMock.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Never);
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Never);
        }

        // --- ESCENARIO 3: Validar la integridad referencial lógica del cliente ---
        [Fact]
        public async Task CreateOrder_WhenCustomerDoesNotExist_ThrowsCustomerNotFoundException()
        {
            // Arrange
            var createOrderRequest = new OrderModel.Request(
                CustomerId: Guid.NewGuid(), // ID que no existe
                ShippingAddress: "Calle Falsa",
                BillingAddress: "Calle Falsa",
                OrderItems: new List<OrderItemModel.Request>()
            );

            // Simulamos que el cliente NO existe devolviendo null
            _repositoryMock.Setup(repo => repo.GetById<Customer>(It.IsAny<Guid>(), It.IsAny<string[]>()))
                           .ReturnsAsync((Customer?)null);

            // Act
            Func<Task> act = async () => await _orderService.AddOrder(createOrderRequest);

            // Assert - Tu código lanza BadRequestException cuando no encuentra al cliente
            await act.Should().ThrowAsync<BadRequestException>()
                     .WithMessage("*No se encontró un cliente con el ID*");
            
            // No debe pasar a validar productos ni guardar nada
            _repositoryMock.Verify(repo => repo.Update(It.IsAny<Product>()), Times.Never);
            _repositoryMock.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Never);
        }
    }
}