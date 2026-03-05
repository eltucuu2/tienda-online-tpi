using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Domain.Entities;
using Dsw2025Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Dsw2025Tpi.Application.Helpers;
using Dsw2025Tpi.Application.Interfaces;

namespace Dsw2025Tpi.Application.Services
{
    public class ProductsManagementService : IProductsManagementService
    {
        private readonly IRepository _repository;
        private readonly ILogger<ProductsManagementService> _logger;
        private readonly ProductsManagmentServiceExtensions _extensions;

        // Constructor con inyección de dependencias
        public ProductsManagementService(IRepository repository, ILogger<ProductsManagementService> logger, ProductsManagmentServiceExtensions extensions)
        {
            _repository = repository;
            _logger = logger;
            _extensions = extensions;
        }

        // Crea un nuevo producto a partir de la solicitud recibida
        public async Task<ProductModel.Response> AddProduct(ProductModel.Request request)
        {
            _logger.LogInformation("Iniciando creación de producto con SKU: {Sku}", request.Sku);

            // Validación del request
            _extensions.ValidateProductRequest(request);

            // Verifica duplicados
            await _extensions.ValidateDuplicatedProductAsync(request);

            // Crea la entidad y la guarda
            var product = new Product(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentPrice, request.StockQuantity);
            await _repository.Add(product);

            _logger.LogInformation("Producto creado correctamente: {Sku}", product.Sku);

            // Devuelve la respuesta con los datos del nuevo producto
            // LÍNEA 47 NUEVA (Agregamos product.Id al principio):
return new ProductModel.Response(product.Id, product.Sku, product.InternalCode, product.Name, product.Description, product.CurrentPrice, product.StockQuantity, product.IsActive);
        }

        // Obtiene la lista de todos los productos existentes
        // ... imports existentes ...

        public async Task<ProductModel.ResponsePagination?> GetProducts(ProductModel.FilterProduct filter)
        {
            _logger.LogInformation("Obteniendo productos con filtros: {@Filter}", filter);

            // 1. Filtro de estado
            bool? isActiveFilter = filter.Status?.ToLower() switch
            {
                "enabled" => true,
                "disabled" => false,
                _ => null
            };

            // 2. Predicado de búsqueda
            System.Linq.Expressions.Expression<Func<Product, bool>> predicate = p =>
                (isActiveFilter == null || p.IsActive == isActiveFilter) &&
                (string.IsNullOrEmpty(filter.Search) || p.Name.Contains(filter.Search) || p.Sku.Contains(filter.Search));

            // 3. Obtener datos de la BD
            var filteredProducts = await _repository.GetFiltered<Product>(predicate);

            // Validación opcional: si no quieres lanzar error 404/204 cuando busca y no encuentra, comenta esta línea:
            // _extensions.ValidateProductsNull(filteredProducts); 

            if (filteredProducts == null || !filteredProducts.Any())
            {
                return new ProductModel.ResponsePagination(new List<ProductModel.Response>(), 0);
            }

            var totalRecords = filteredProducts.Count();

            // 4. VALIDACIÓN DE PAGINACIÓN (Aquí está la corrección clave)
            // Aseguramos que la página sea al menos 1 y el tamaño al menos 1
            var page = filter.PageNumber < 1 ? 1 : filter.PageNumber;
            var size = filter.PageSize < 1 ? 10 : filter.PageSize;

            // 5. Aplicar Paginación
            var pagedList = filteredProducts
                .OrderBy(p => p.Name)
                .Skip((page - 1) * size) // (1-1)*5 = 0. Skip(0) es correcto.
                .Take(size)
                .Select(p => _extensions.ToResponse(p))
                .ToList();

            return new ProductModel.ResponsePagination(pagedList, totalRecords);
        }

        // Obtiene un producto por su ID
        public async Task<Product?> GetProductById(Guid id)
        {
            _logger.LogInformation("Buscando producto por ID: {Id}", id);

            // Lanza excepción si no se encuentra
            var product = await _extensions.GetProductOrThrow(id);

            _logger.LogInformation("Producto encontrado: {Sku}", product.Sku);

            return product;
        }

        // Actualiza los datos de un producto
        public async Task<ProductModel.Response> Update(Guid Id, ProductModel.Request request)
        {
            _logger.LogInformation("Actualizando producto con ID: {Id}", Id);

            _extensions.ValidateProductRequest(request);

            var product = await _repository.GetById<Product>(Id);
            _extensions.ValidateProductNull(product);

            await _extensions.ValidateDuplicatedProductUpdateAsync(request, Id);

            // Aplica los cambios
            product.Update(request.Sku, request.InternalCode, request.Name, request.Description, request.CurrentPrice, request.StockQuantity);
            await _repository.Update(product);

            _logger.LogInformation("Producto actualizado correctamente. SKU: {Sku}", product.Sku);

            return _extensions.ToResponse(product);
        }

        // Desactiva (soft delete) un producto existente
        public async Task<bool> DisableProduct(Guid Id)
        {
            _logger.LogInformation("Desactivando producto con ID: {Id}", Id);

            var product = await _repository.GetById<Product>(Id);
            _extensions.ValidateProductNull(product);

            // Cambia el estado a inactivo
            product.IsActive = false;
            await _repository.Update(product);

            _logger.LogInformation("Producto desactivado correctamente. SKU: {Sku}", product.Sku);
            return true;
        }
    }
}

