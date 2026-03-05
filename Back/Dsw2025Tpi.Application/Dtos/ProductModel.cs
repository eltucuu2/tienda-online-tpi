using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    // Modelo para representar un producto dentro de la aplicación
    public record ProductModel
    {
        // DTO para solicitudes (crear o actualizar un producto)
        public record Request(
            string Sku,            // Código único del producto (Stock Keeping Unit)
            string InternalCode,   // Código interno para control interno
            string Name,           // Nombre del producto
            string Description,    // Descripción del producto
            decimal CurrentPrice,  // Precio actual del producto
            int StockQuantity      // Cantidad disponible en stock
        );

        // DTO para respuestas (mostrar datos del producto)
        public record Response(
            Guid Id,
            string? Sku,           // Código único del producto (puede ser nulo)
            string? InternalCode,  // Código interno del producto (puede ser nulo)
            string? Name,          // Nombre del producto (puede ser nulo)
            string? Description,   // Descripción del producto (puede ser nulo)
            decimal? CurrentPrice, // Precio actual del producto (puede ser nulo)
            int? StockQuantity,    // Cantidad en stock (puede ser nulo)
            bool? IsActive         // Estado de actividad del producto (true = activo, false = deshabilitado)
        );

        // DTO para recibir los filtros desde el Controller
        public record FilterProduct(string? Status, string? Search, int PageNumber, int PageSize);

        // DTO para devolver la lista paginada y el total
        public record ResponsePagination(IEnumerable<Response> ProductItems, int Total);
    }
}

