using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Domain.Entities;

namespace Dsw2025Tpi.Application.Interfaces
{
    public interface IOrdersManagementService
    {
        Task<OrderModel.Response> AddOrder(OrderModel.Request request);
        Task<OrderModel.ResponsePagination> GetOrders(OrderStatus? status, Guid? customerId, int pageNumber, int pageSize);
        Task<Order?> GetOrderById(Guid id);
        Task<OrderModel.Response?> UpdateOrderStatus(Guid id, string newStatusText);
    }
}