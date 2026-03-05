using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dsw2025Tpi.Domain.Entities
{
    public class Order : EntityBase
    {
        public DateTime Date { get; set; }
        public string? ShippingAddress { get; set; }
        public string? BillingAddress { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public ICollection<OrderItem> Items { get; set; } = new HashSet<OrderItem>();

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        
        [NotMapped]
        public string? CustomerName => Customer?.Name;

        public Order() { }

        public Order(Guid Customerid, string shippingAddress, string billingAddress, ICollection<OrderItem> items)
        {
            CustomerId = Customerid;
            ShippingAddress = shippingAddress;
            BillingAddress = billingAddress;
            Items = items;
            Date = DateTime.UtcNow;
            Status = OrderStatus.PENDING;
            TotalAmount = items.Sum(i => i.Subtotal);
        }
    }
}
