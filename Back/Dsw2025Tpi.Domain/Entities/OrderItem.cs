using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Domain.Entities
{
    // Clase que representa un ítem o línea dentro de una orden
    public class OrderItem : EntityBase
    {
        private Product product;

        // Cantidad de unidades del producto en este ítem
        public int Quantity { get; set; }

        // Precio unitario del producto al momento de la compra
        public decimal UnitPrice { get; set; }

        // Subtotal calculado para este ítem (Quantity * UnitPrice)
        public decimal Subtotal { get; set; }

        // Clave foránea que indica a qué orden pertenece este ítem
        public Guid OrderId { get; set; }

        // Clave foránea que indica qué producto es este ítem
        public Guid ProductId { get; set; }

        // Propiedad de navegación para acceder a los datos del producto
        public Product? Product { get; set; }

        // Propiedad de navegación para acceder a la orden que contiene este ítem
        public Order? Order { get; set; }

        // Constructor vacío requerido por EF y para serialización
        public OrderItem() { }

        // Constructor que inicializa un ítem con producto, cantidad y precio unitario
        public OrderItem(Product product, Guid productId, int quantity, decimal unitPrice)
        {
            if (productId == Guid.Empty)
                throw new ArgumentNullException(nameof(productId), "El Id del producto no puede estar vacío.");

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException(nameof(quantity), "La cantidad debe ser mayor a 0");

            if (!product.StockControl(quantity))
                throw new ApplicationException("No hay stock suficiente");

            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            Subtotal = quantity * unitPrice; // Calcula el subtotal al crear el ítem
        }

        public OrderItem(Product product, int quantity)
        {
            this.product = product;
            Quantity = quantity;
        }
    }
}
