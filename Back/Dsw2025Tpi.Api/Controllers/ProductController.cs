using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Interfaces;
using Dsw2025Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    // Indica que esta clase es un controlador de API
    [ApiController]

    // Requiere autenticación en todos los métodos por defecto
    [Authorize]

    // Ruta base para los endpoints de este controlador
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        // Servicio de lógica de productos
        private readonly IProductsManagementService _service;

        // Constructor con inyección de dependencia del servicio
        public ProductsController(IProductsManagementService service)
        {
            _service = service;
        }

        // === POST: api/products ===
        // Agrega un nuevo producto (solo rol Admin)
        [HttpPost()]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel.Request request)
        {
            // Agrega el producto usando el servicio
            var product = await _service.AddProduct(request);

            // Retorna 201 Created con la ubicación del nuevo producto
            return Created($"/api/products/{product.Sku}", product);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAuthProducts([FromQuery] ProductModel.FilterProduct request)
        {
            var result = await _service.GetProducts(request);

            // Si result es null o vacío, podrías devolver NoContent o la estructura vacía.
            // Según tu lógica actual en servicio, devolvemos la estructura con total 0.
            
            return Ok(result);
        }
        
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProducts()
        {
            // Ejemplo: Traer solo activos, página 1, 100 productos
            var filter = new ProductModel.FilterProduct("enabled", null, 1, 100);
            var result = await _service.GetProducts(filter);
            return Ok(result?.ProductItems); // Retorna solo la lista para compatibilidad simple
        }

        // === GET: api/products/{id} ===
        // Devuelve un producto por su ID (solo Admin)
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProductBySku(Guid id)
        {
            // Busca el producto por su ID
            var product = await _service.GetProductById(id);

            // Retorna los campos del producto
            var result = new
            {
                product.Id,
                product.Sku,
                product.InternalCode,
                product.Name,
                product.Description,
                product.CurrentPrice,
                product.StockQuantity,
                product.IsActive
            };

            return Ok(result);
        }

        // === PUT: api/products/{id} ===
        // Actualiza todos los campos del producto (solo Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] ProductModel.Request request)
        {
            // Llama al servicio para actualizar el producto
            var updatedProduct = await _service.Update(id, request);

            // Retorna 200 OK con el producto actualizado
            return Ok(updatedProduct);
        }

        // === PATCH: api/products/{id} ===
        // Deshabilita (soft delete) un producto (solo Admin)
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        
        public async Task<IActionResult> DisableProduct(Guid id)
        {
            // Llama al servicio para deshabilitar el producto
            var success = await _service.DisableProduct(id);

            // Si se deshabilitó correctamente, retorna 204 sin contenido
            return NoContent();
        }
    }
}
