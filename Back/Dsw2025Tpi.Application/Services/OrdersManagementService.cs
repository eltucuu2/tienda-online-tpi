using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Helpers;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class OrdersManagementService : IOrdersManagementService
    {
        private readonly IRepository _repository;
        private readonly ILogger<OrdersManagementService> _logger;
        private readonly OrdersManagementServiceExtensions _extensions;

        public OrdersManagementService(IRepository repository, ILogger<OrdersManagementService> logger,
            OrdersManagementServiceExtensions extensions)
        {
            _repository = repository;
            _logger = logger;
            _extensions = extensions;
        }

        // -------------------------
        // CREAR ORDEN
        // -------------------------
        public async Task<OrderModel.Response> AddOrder(OrderModel.Request request)
        {
            await _extensions.ValidateIdCustomerAsync(request);

            _logger.LogInformation("Iniciando creación de orden para cliente {CustomerId}", request.CustomerId);

            _extensions.ValidateOrderRequest(request);
            _extensions.ValidateEmptyProducts(request);
            await _extensions.ValidateStockAvailabilityAsync(request);

            var productsList = await _extensions.ValidateProductsInListAsync(request);
            var products = productsList.ToDictionary(p => p.Id);

            var orderItems = request.OrderItems.Select(i =>
            {
                var product = products[i.ProductId];
                return new OrderItem(product, product.Id, i.Quantity, product.CurrentPrice);
            }).ToList();

            foreach (var item in orderItems)
            {
                var product = products[item.ProductId];
                product.StockQuantity -= item.Quantity;
                await _repository.Update(product);
            }

            var fecLocArg = _extensions.GetDateArgentinean();

            var order = new Order(request.CustomerId, request.ShippingAddress, request.BillingAddress, orderItems)
            {
                Date = fecLocArg
            };

            await _repository.Add(order);

            _logger.LogInformation("Orden creada exitosamente {OrderId}", order.Id);

            return new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                CustomerName: order.CustomerName,   // ← FIX
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date,
                TotalAmount: order.TotalAmount,
                Status: order.Status.ToString(),
                OrderItems: order.Items.Select(oi =>
                {
                    var product = products[oi.ProductId];
                    return new OrderItemModel.Response(
                        ProductId: oi.ProductId,
                        Name: product.Name ?? string.Empty,
                        Description: product.Description ?? string.Empty,
                        UnitPrice: oi.UnitPrice,
                        Quantity: oi.Quantity,
                        Subtotal: oi.Subtotal
                    );
                }).ToList()
            );
        }

        // -------------------------
        // LISTAR ORDENES
        // -------------------------
        public async Task<OrderModel.ResponsePagination> GetOrders(OrderStatus? status, Guid? customerId, int pageNumber, int pageSize)
        {
            Expression<Func<Order, bool>> filter = o =>
                (!status.HasValue || o.Status == status.Value) &&
                (!customerId.HasValue || o.CustomerId == customerId.Value);

            // Se agregó "Customer" para que order.Customer no sea null
            var allOrders = await _repository.GetFiltered<Order>(filter, new[] { "Items", "Customer" });

            if (allOrders == null || !allOrders.Any())
                return new OrderModel.ResponsePagination(new List<OrderModel.Response>(), 0);

            var totalRecords = allOrders.Count();

            var page = pageNumber < 1 ? 1 : pageNumber;
            var size = pageSize < 1 ? 10 : pageSize;

            var pagedOrders = allOrders
                .OrderByDescending(o => o.Date)
                .Skip((page - 1) * size)
                .Take(size)
                .ToList();

            var productIds = pagedOrders
                .SelectMany(o => o.Items)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            var products = productsList.ToDictionary(p => p.Id);

            var responseList = pagedOrders.Select(order => new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                CustomerName: order.Customer?.Name ?? string.Empty,    // ← FIX seguro
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date,
                TotalAmount: order.TotalAmount,
                Status: order.Status.ToString(),
                OrderItems: order.Items.Select(oi =>
                {
                    var product = products.GetValueOrDefault(oi.ProductId);
                    return new OrderItemModel.Response(
                        ProductId: oi.ProductId,
                        Name: product?.Name ?? string.Empty,
                        Description: product?.Description ?? string.Empty,
                        UnitPrice: oi.UnitPrice,
                        Quantity: oi.Quantity,
                        Subtotal: oi.Subtotal
                    );
                }).ToList()
            )).ToList();

            return new OrderModel.ResponsePagination(responseList, totalRecords);
        }

        // -------------------------
        // OBTENER ORDEN POR ID
        // -------------------------
        public async Task<Order?> GetOrderById(Guid Id)
        {
            var orders = await _repository.GetFiltered<Order>(
                o => o.Id == Id,
                include: new[] { "Items", "Items.Product", "Customer" } // <-- agregado Customer
            );

            var order = _extensions.ValidateOrderNull(Id, orders);

            return order;
        }

        // -------------------------
        // ACTUALIZAR ESTADO
        // -------------------------
        public async Task<OrderModel.Response?> UpdateOrderStatus(Guid id, string newStatusText)
        {
            var newStatusTextUpper = newStatusText.ToUpper();

            var order = await _repository.GetById<Order>(id, "Items,Customer"); // <-- agregado Customer
            if (order == null)
                throw new NotFoundException("La orden solicitada no existe");

            if (int.TryParse(newStatusTextUpper, out _))
                throw new BadRequestException("No se permite un número como estado.");

            if (!Enum.TryParse<OrderStatus>(newStatusTextUpper, true, out var newStatus))
                throw new BadRequestException("Estado inválido.");

            if (order.Status != newStatus)
            {
                order.Status = newStatus;
                await _repository.Update(order);
            }

            var productIds = order.Items.Select(i => i.ProductId).Distinct().ToList();
            var productsList = await _repository.GetFiltered<Product>(p => productIds.Contains(p.Id));
            var products = productsList.ToDictionary(p => p.Id);

            return new OrderModel.Response(
                Id: order.Id,
                CustomerId: order.CustomerId ?? Guid.Empty,
                CustomerName: order.Customer?.Name ?? string.Empty,     // ← FIX seguro
                ShippingAddress: order.ShippingAddress,
                BillingAddress: order.BillingAddress,
                Date: order.Date,
                TotalAmount: order.TotalAmount,
                Status: order.Status.ToString(),
                OrderItems: order.Items.Select(oi =>
                {
                    var product = products.GetValueOrDefault(oi.ProductId);
                    return new OrderItemModel.Response(
                        ProductId: oi.ProductId,
                        Name: product?.Name ?? string.Empty,
                        Description: product?.Description ?? string.Empty,
                        UnitPrice: oi.UnitPrice,
                        Quantity: oi.Quantity,
                        Subtotal: oi.Subtotal
                    );
                }).ToList()
            );
        }
    }
}
