using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Helpers
{
    public class ProductsManagmentServiceExtensions
    {
        private readonly IRepository _repository;
        private readonly ILogger<ProductsManagmentServiceExtensions> _logger;
        public ProductsManagmentServiceExtensions(
            IRepository repository,
            ILogger<ProductsManagmentServiceExtensions> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public void ValidateProductRequest(ProductModel.Request request)
        {
            if (string.IsNullOrWhiteSpace(request.Sku) ||
                string.IsNullOrWhiteSpace(request.Name)
                || string.IsNullOrWhiteSpace(request.InternalCode))
            {
                _logger.LogWarning("Datos inválidos del producto: {@Request}", request);
                throw new BadRequestException("Los datos del producto no son válidos.");
            }
        }

        public async Task<Product> GetProductOrThrow(Guid id)
        {
            var product = await _repository.GetById<Product>(id);
            if (product == null)
            {
                _logger.LogWarning("Producto no encontrado con ID: {Id}", id);
                throw new NotFoundException($"No se encontró un producto con el ID: {id}.");
            }
            return product;
        }

        public ProductModel.Response ToResponse(Product product)
        {
            return new ProductModel.Response(
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentPrice,
                product.StockQuantity,
                product.IsActive
            );
        }

        public void ValidateProductNull(Product? product)
        {
            if (product == null)
            {
                _logger.LogWarning("Producto no encontrado en la base de datos");
                throw new NotFoundException("Producto no encontrado en la base de datos.");
            }
        }

        public void ValidateProductsNull(IEnumerable<Product>? products)
        {
            if (products == null || !products.Any())
            {
                _logger.LogWarning("No hay productos disponibles en la base de datos");
                throw new NoContentException("No hay productos disponibles en la base de datos.");
            }
        }
        public async Task ValidateDuplicatedProductAsync(ProductModel.Request request)
        {
            var exist = await _repository.First<Product>(p => p.Sku == request.Sku);
            if (exist != null)
            {
                _logger.LogWarning("Intento de duplicación de producto con SKU existente: {Sku}", request.Sku);
                throw new BadRequestException($"Ya existe un producto con el Sku {request.Sku}");
            }
        }

        public async Task ValidateDuplicatedProductUpdateAsync(ProductModel.Request request, Guid Id)
        {
            var exist = await _repository.First<Product>(p =>
            p.Sku == request.Sku && p.Id != Id);

            if (exist != null)
            {
                _logger.LogWarning("Intento de duplicación de producto con SKU existente: {Sku}", request.Sku);
                throw new BadRequestException($"Ya existe un producto con el Sku {request.Sku}");
            }
        }
    }
}
