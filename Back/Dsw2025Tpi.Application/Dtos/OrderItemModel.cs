using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Dtos
{
    // Modelo que agrupa los DTOs relacionados con los ítems de una orden
    public record OrderItemModel
    {
        // DTO que se utiliza para la solicitud (Request) de creación o carga de un ítem de orden.
        public record Request(
            Guid ProductId,          // ID del producto (clave primaria del producto)
            int Quantity
        );

        // DTO que se utiliza para la respuesta (Response) de un ítem de orden ya procesado.
        public record Response(
            Guid ProductId,      // ID del producto
            string? Name,        // Nombre del producto (puede ser nulo)
            string? Description, // Descripción del producto (puede ser nula)
            decimal? UnitPrice,  // Precio unitario (puede ser nulo, si no está definido)
            int? Quantity,       // Cantidad del producto (puede ser nula)
            decimal? Subtotal    // Subtotal calculado = Precio * Cantidad (puede ser nulo)
        );
    }
}
