using Dsw2025Tpi.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Dsw2025Tpi.Application.Dtos
{
    public record OrderModel
    {
        public record Request(
            Guid CustomerId,
            string ShippingAddress,
            string BillingAddress,
            List<OrderItemModel.Request> OrderItems
        );

        public record UpdateOrderStatusRequest(
            string NewStatus
        );

        public record Response(
            Guid Id,
            Guid CustomerId,
            string? CustomerName,
            string? ShippingAddress,
            string? BillingAddress,
            DateTime Date,
            decimal? TotalAmount,
            string? Status,
            List<OrderItemModel.Response> OrderItems
        );

        public record ResponsePagination(IEnumerable<Response> OrderItems, int Total);
    }
}
