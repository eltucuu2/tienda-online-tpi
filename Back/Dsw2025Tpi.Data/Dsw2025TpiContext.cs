using Dsw2025Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dsw2025Tpi.Data;

// Contexto principal de EF Core para la aplicación, hereda de DbContext
public class Dsw2025TpiContext : DbContext
{
    // Constructor que recibe las opciones de configuración (cadena de conexión, etc.)
    public Dsw2025TpiContext(DbContextOptions<Dsw2025TpiContext> options) : base(options)
    {
    }

    // Configuración del modelo y las entidades
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración para la entidad Product
        modelBuilder.Entity<Product>(p =>
        {
            p.HasKey(p => p.Id);  // Llave primaria
            p.Property(p => p.Sku).IsRequired().HasMaxLength(15);         // SKU obligatorio, max 15 caracteres
            p.Property(p => p.Name).IsRequired().HasMaxLength(60);        // Nombre obligatorio, max 60 caracteres
            p.Property(p => p.InternalCode).IsRequired().HasMaxLength(30); // Código interno obligatorio, max 30 caracteres
            p.Property(p => p.Description).HasMaxLength(500);             // Descripción opcional, max 500 caracteres
            p.Property(p => p.CurrentPrice).IsRequired().HasPrecision(15, 2); // Precio con precisión decimal 15,2
            p.Property(p => p.StockQuantity).IsRequired();                // Cantidad en stock obligatoria
            p.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);  // Activo por defecto true
        });

        // Configuración para la entidad Order (Pedido)
        modelBuilder.Entity<Order>(o =>
        {
            o.HasKey(o => o.Id); // Llave primaria
            o.HasOne(o => o.Customer) // Relación con Customer
                .WithMany(c => c.Orders) // Un cliente puede tener muchas órdenes
                .HasForeignKey(o => o.CustomerId) // FK en Order
                .OnDelete(DeleteBehavior.SetNull); // Si cliente se elimina, dejar null en Order.CustomerId
            o.Property(o => o.Date).IsRequired(); // Fecha obligatoria
            o.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(200); // Dirección envío obligatoria
            o.Property(o => o.BillingAddress).IsRequired().HasMaxLength(200);  // Dirección facturación obligatoria
            o.Property(o => o.Notes).HasMaxLength(500);                        // Notas opcionales
            o.Property(o => o.Status).IsRequired();                            // Estado del pedido obligatorio
            o.Property(o => o.TotalAmount).IsRequired().HasPrecision(15, 2);  // Total pedido con precisión decimal
        });

        // Configuración para la entidad OrderItem (Detalle de pedido)
        modelBuilder.Entity<OrderItem>(oi =>
        {
            oi.HasKey(oi => oi.Id); // Llave primaria
            oi.HasOne(oi => oi.Order) // Relación con Order
                .WithMany(o => o.Items) // Un pedido tiene muchos items
                .HasForeignKey(o => o.OrderId) // FK en OrderItem
                .OnDelete(DeleteBehavior.Cascade); // Al borrar pedido, borrar items
            oi.HasOne(oi => oi.Product) // Relación con Product
                .WithMany(p => p.OrderItems) // Un producto puede estar en muchos items
                .HasForeignKey(oi => oi.ProductId) // FK en OrderItem
                .OnDelete(DeleteBehavior.Restrict); // No permite borrar producto si hay items asociados
            oi.Property(oi => oi.Quantity).IsRequired(); // Cantidad obligatoria
            oi.Property(oi => oi.UnitPrice).IsRequired().HasPrecision(15, 2); // Precio unitario con precisión decimal
            oi.Property(oi => oi.Subtotal).HasPrecision(15, 2); // Subtotal con precisión decimal
        });

        // Configuración para la entidad Customer (Cliente)
        modelBuilder.Entity<Customer>(c =>
        {
            c.HasKey(c => c.Id); // Llave primaria
            c.Property(c => c.Id).IsRequired(); // Id obligatorio
            c.Property(c => c.Name).IsRequired().HasMaxLength(60); // Nombre obligatorio, max 60 caracteres
            c.Property(c => c.Email).IsRequired().HasMaxLength(100); // Email obligatorio, max 100 caracteres
            c.Property(c => c.Phone).HasMaxLength(15); // Teléfono opcional, max 15 caracteres
        });
    }
}
