using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Domain.Entities
{
    // Representa un producto que puede ser vendido y tiene stock disponible
    public class Product : EntityBase
    {
        // Código SKU único para identificar el producto
        public string? Sku { get; set; }

        // Código interno utilizado para referencia interna
        public string? InternalCode { get; set; }

        // Nombre descriptivo del producto
        public string? Name { get; set; }

        // Descripción detallada del producto
        public string? Description { get; set; }

        // Precio actual del producto
        public decimal CurrentPrice { get; set; }

        // Cantidad disponible en stock (nullable para permitir no definido)
        public int? StockQuantity { get; set; }

        // Indica si el producto está activo y disponible para venta
        public bool IsActive { get; set; }

        // Colección de elementos de orden que incluyen este producto
        public ICollection<OrderItem> OrderItems { get; } = new HashSet<OrderItem>();

        // Constructor por defecto
        public Product() { }

        // Constructor que inicializa las propiedades clave del producto
        public Product(string sku, string internalCode, string name, string description, decimal currentPrice, int stockQuantity)
        {
            Sku = sku;
            InternalCode = internalCode;
            Name = name;
            Description = description;
            CurrentPrice = currentPrice <= 0 ?
                throw new ArgumentOutOfRangeException(nameof(currentPrice), "El precio no debe ser menor o igual a 0") : currentPrice;
            StockQuantity = stockQuantity <= 0 ?
                throw new ArgumentOutOfRangeException(nameof(stockQuantity), "El stock no debe ser menor o igual a 0") : stockQuantity;
            IsActive = true; // Por defecto el producto está activo
        }

        public void Update(string sku, string internalCode, string name, string description, decimal currentPrice, int? stockQuantity)
        {
            Sku = sku;
            InternalCode = internalCode;
            Name = name;
            Description = description;
            // La validación debe estar aquí para que se ejecute al actualizar
            CurrentPrice = currentPrice <= 0 ?
                throw new ArgumentOutOfRangeException(nameof(currentPrice), "El precio no debe ser menor o igual a 0") : currentPrice;
            StockQuantity = stockQuantity <= 0 ?
                throw new ArgumentOutOfRangeException(nameof(stockQuantity), "El stock no debe ser menor o igual a 0") : stockQuantity;
        }

        public bool StockControl (int quantity)
        {
            if (quantity < StockQuantity)
            {
                StockQuantity -= quantity;
                return true;
            }

            return false;
        }
    }
}
