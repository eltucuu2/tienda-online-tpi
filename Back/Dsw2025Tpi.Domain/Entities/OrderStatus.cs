using System;

namespace Dsw2025Tpi.Domain.Entities
{
    // Enum que representa los posibles estados de una orden
    public enum OrderStatus
    {
        PENDING = 1,     // Orden creada y pendiente de procesamiento
        PROCESSING = 2,  // Orden en proceso de preparación o embalaje
        SHIPPED = 3,     // Orden enviada al cliente
        DELIVERED = 4,   // Orden entregada al cliente
        CANCELED = 5     // Orden cancelada y no completada
    }
}
